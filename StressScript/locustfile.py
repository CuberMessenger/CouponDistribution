from locust import TaskSet, between, task, HttpLocust
from locust.contrib.fasthttp import FastHttpLocust
import json
import uuid
import random
from threading import Lock
import os

baseURL = "http://localhost:20080"

def register(client, kind):
    username = str(uuid.uuid4())[:6]
    password = str(uuid.uuid4())[10:]
    try:
        res = client.post(baseURL+"/api/users", data=json.dumps({"username": username, "password": password, "kind": kind}),
            headers={"content-type": "application/json"}, name="/api/users")
        print('username: {}'.format(username))
        print('password: {}'.format(password))
        print(res.status_code)
        print(res.json())
    except:
        return "", ""
    if res.ok:
        return username, password
    return "", ""

def login(client, loginMsg):
    try:
        res = client.post(baseURL+"/api/auth", data=json.dumps({"username":loginMsg["username"], "password":loginMsg["password"]}),
            headers={"Content-Type": "application/json"}, name="/api/auth")
    except:
        return ""
    if res.ok:
        return res.headers["Authorization"]
    return ""

def add(client, username, token):
    amount = random.randint(1, 5)
    name = str(uuid.uuid4())
    stock = random.randint(1, 500)
    try:
        res = client.post(baseURL+"/api/users/"+username+"/coupons", json={"name": name, "amount": amount, "stock": stock},
            headers={"Authorization": token,"content-type": "application/json",}, name="/api/users/[username]/coupons")
    except:
        return "", -1
    if res.ok:
        return name, amount
    return "", -1 

class UserBehavior(TaskSet):
    tokens = {}
    coupons = ""
    customers = []
    saler = ""
    def on_start(self):
        # add saler
        while True:
            username, password = register(self.locust.client, "saler")
            if username == "":
                continue
            token = login(self.locust.client, {"username": username, "password": password})
            if token == "":
                continue
            print("register saler {}.".format(username))
            self.saler = username
            self.tokens[username] = token
            break
        # add coupons
        while True:
            name, amount = add(self.locust.client, self.saler, self.tokens[self.saler])
            if amount > 0:
                print("saler {} add coupons".format(self.saler))
                self.coupons = name
                break
        # add customer
        max = random.randint(10, 15)
        cnt = 0
        while True:
            if cnt >= max:
                break
            username, password = register(self.locust.client, "customer")
            if username == "":
                continue
            token = login(self.locust.client, {"username": username, "password": password})
            if token == "":
                continue
            print("add customer {} for saler {}.".format(username, self.saler))
            self.customers.append(username)
            self.tokens[username] = token
            cnt += 1
    
    @task(9)
    def search(self):
        saler = self.saler
        token = self.tokens[self.customers[random.randint(0, len(self.customers)-1)]]
        res = self.client.get("/api/users/"+saler+"/coupons", data=json.dumps({'page': '1'}), headers={"Authorization": token, 'content-type': 'application/json'}, name="/api/users/[username]/coupons")
        if res.ok:
            return True
        return False
    
    @task(1)
    def get(self):
        saler = self.saler
        coupon = self.coupons
        token = self.tokens[self.customers[random.randint(0, len(self.customers)-1)]]
        res = self.client.patch("/api/users/"+saler+"/coupons/"+coupon, headers={"Authorization": token}, name="/api/users/[username]/coupons/[coupon]")
        print(res.json())
        if res.ok:
            return True
        return False


class User(HttpLocust):
    task_set = UserBehavior
    wait_time = between(0.01, 0.02)

if __name__ == "__main__":
    # start load test
    os.system("locust -f locustfile.py --host="+baseURL)
        