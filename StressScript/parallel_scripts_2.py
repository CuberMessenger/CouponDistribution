import gevent.monkey
gevent.monkey.patch_all()

import requests
import time
from requests_toolbelt import MultipartEncoder
import json
from locust import HttpLocust,TaskSet,task,between
# from names_dataset import NameDataset
import random

requests.packages.urllib3.disable_warnings()

baseUrl = "http://localhost:20080"
userApi = "/api/users"
authApi = "/api/auth"
couponsApi = "/coupons"

class Person(object):
    def __init__(self, username="", password=""):
        self.username = str(username)
        self.password = str(password)
        self.authorization = "null"
        self.kind = "customer"

    def get_registration_dict(self):
        return {"username": self.username, "password": self.password, "kind": self.kind}

    def get_login_dict(self):
        return {"username": self.username, "password": self.password}


class Sales(Person):
    def __init__(self, username="", password=""):
        Person.__init__(self, username, password)
        self.kind = "saler"

    def __repr__(self):
        return "Sales[username=" + self.username + ", password=" + self.password \
               + ", authorization=" + self.authorization + "]"


class Customer(Person):
    def __init__(self, username="", password=""):
        Person.__init__(self, username, password)
        self.amount = ""
        self.left = ""
        self.kind = "customer"

    def __repr__(self):
        return "Customer[username=" + self.username + ", password=" + self.password \
               + ", authorization=" + self.authorization + "]"


class Coupon(object):
    def __init__(self, name, amount, stock, description="This coupon has no description."):
        self.name = name
        self.amount = amount
        self.stock = stock
        self.description = description

    def get_coupons_dict(self):
        return {"name": self.name, "amount": self.amount, "description": self.description, "stock": self.stock}

    def __repr__(self):
        return "Coupons[name=" + self.name + ", amount=" \
               + self.amount + ", stock=" + self.stock \
               + ", description=" + self.description + "]"


class Test(object):
    def __init__(self, session=requests.session()):
        self.url = baseUrl
        self.response = session
        self.msg = None


class TestRegistration(Test):
    def __init__(self, user, session):
        Test.__init__(self, session)
        self.user = user
        self.url += userApi
        self.register()

    def register(self):
        s = self.response.post(self.url,
                            data=json.dumps(self.user.get_registration_dict()),
                            headers={"content-type": "application/json"}, verify = False)
        if s.status_code == 204:
            print("status-code: 204, empty message.")
        elif s.status_code == 200:
            print("{} Registration successed".format(self.user.username))
        else:
            print("Error message:", self.user.username, s.json()["errMsg"])
        # if s.ok:
        #     if s.json()["errMsg"] == "":
        #         self.msg = 200
        #         print("注册成功:", self.user.username)
        #     else:
        #         self.msg = 201
        #         print("认证成功但用户已注册:", self.user.username)
        # else:
        #     self.msg = s.status_code
        #     print("认证失败:", self.user.username)

class TestLogin(Test):
    def __init__(self, user, session):
        Test.__init__(self, session)
        self.user = user
        self.url += authApi
        self.login()

    def login(self):
        # s = self.response.post(self.url, data=self.user.get_login_dict())
        # self.msg = s.status_code
        # if s.status_code == 200:
        #     self.user.authorization = s.headers["Authorization"]
        #     print("登录成功:", self.user.username)
        # elif s.status_code == 401:
        #     print("认证失败（用户不存在或密码错误）:", self.user.username)
        # else:
        #     print("服务端错误")
        s = self.response.post(self.url,
                            data=json.dumps(self.user.get_login_dict()),
                            headers={"content-type": "application/json"}, verify = False)
        if s.status_code == 204:
            print("status-code: 204, empty message.")
        elif s.status_code == 200:
            self.user.authorization = s.headers["Authorization"]
            print("{} User login".format(self.user.username))
        else:
            print("Error message:" , s.json()["errMsg"])


class TestAddCoupons(Test):
    def __init__(self, user, coupon, session):
        Test.__init__(self, session)
        self.user = user
        self.coupon = coupon
        self.url += userApi + "/"
        self.generate_new_coupons()

    def generate_new_coupons(self):
        # if self.user.kind == "saler":
        #     url = self.url + self.user.username + couponsApi
        #     s = self.response.post(url, data=self.coupon.get_coupons_dict(),
        #                            headers={"Authorization": self.user.authorization})
        #     # self.coupon.sales = self.user.username
        #     self.msg = s.status_code
        #     if s.status_code == 200:
        #         print(self.user.username + " 新建了 " + str(self.coupon.amount) + " 张 " + self.coupon.name + " Coupons")
        #     elif s.status_code == 401:
        #         print("认证失败:", self.user.username)
        #     elif s.status_code == 400:
        #         print("创建用户原因导致的创建失败或其他问题:", self.user.username)
        #     else:
        #         print("服务端错误")
        # else:
        #     print("非商家，不能新建Coupons")
        url = self.url + self.user.username + couponsApi
        s = self.response.post(url,
                            data=json.dumps(self.coupon.get_coupons_dict()),
                            headers={"content-type": "application/json",
                                    "Authorization": self.user.authorization
                            }, verify = False)
        if s.status_code == 204:
            print("status-code: 204, empty message.")
        elif s.status_code == 201:
            print("[Saler]" + self.user.username + " created " + str(self.coupon.amount) + " [Coupon] " + self.coupon.name)
        else:
            print("error message:" , s.json()["errMsg"])


class TestGetCouponsInfo(Test):
    def __init__(self, searcher, searchee, session):
        Test.__init__(self, session)
        self.searcher = searcher
        self.searchee = searchee
        self.url += userApi + "/"
        self.get_coupons_info()

    def get_coupons_info(self):
        url = self.url + self.searchee.username + couponsApi
        data = MultipartEncoder(fields={"page": "1"})
        headers = {
            "Authorization": self.searcher.authorization,
            'Content-Type': data.content_type
        }
        g = self.response.get(url, data=data, headers=headers, verify = False)
        self.msg = g.status_code
        # print(g.text)
        if self.searchee.kind == "saler":
            if g.status_code == 200:
                if self.searcher.kind == "saler":
                    print("[Sales]" + self.searcher.username + " <===Coupons=== [Sales]" + self.searchee.username)
                elif self.searcher.kind == "customer":
                    print("[Customer]" + self.searcher.username + " <===Coupons=== [Sales]" + self.searchee.username)
            elif g.status_code == 204:
                print("查询商家剩余的优惠券信息，但查询结果为空")
            elif g.status_code == 401:
                print("查询商家:", self.searcher.username)
            elif g.status_code == 400:
                print("查询的用户不存在:", self.searchee.username)
            else:
                print("服务端错误")
        else:
            if g.status_code == 200:
                print("[Customer]" + self.searcher.username + "查询已抢到的优惠券信息")
            elif g.status_code == 204:
                print("查询顾客自己已经抢到的优惠券信息，但查询结果为空")
            elif g.status_code == 401:
                print("试图访问其他顾客的优惠券列表，认证失败:", self.searcher.username)
            elif g.status_code == 400:
                print("查询的用户不存在:", self.searchee.username)
            else:
                print("服务端错误")


class TestCustomerGetCoupons(Test):
    def __init__(self, customer, sales, coupon, session):
        Test.__init__(self, session)
        self.customer = customer
        self.sales = sales
        self.coupon = coupon
        self.url += userApi + "/"
        self.get_coupons_info()

    def get_coupons_info(self):
        url = self.url + self.sales.username + couponsApi + "/" + self.coupon.name
        headers = {
            "Authorization": self.customer.authorization,
        }
        g = self.response.patch(url, headers=headers, verify = False)
        self.msg = g.status_code
        if g.status_code == 201:
            print("[Customer]" + self.customer.username + " got [Coupon]"
                  + self.coupon.name + " from [Sales]" + self.sales.username)
        elif g.status_code == 204:
            print("[Customer]" + self.customer.username + " failed to get [Coupon]"
                  + self.coupon.name + " from [Sales]" + self.sales.username +
                  ", because he had one, or [Sales]" + self.sales.username + "don't have this coupon.")
        elif g.status_code == 401:
            print("[Customer]" + self.customer.username + "认证失败")
        else:
            print("服务端错误")

# 高并发压力测试
class TestUser(TaskSet):

    @task(1)
    def register(self):
        persons, _ = generate_test_data()
        user = persons[random.randint(0, len(persons))]
        url = baseUrl + userApi

        s = self.client.post(url, data=user.get_registration_dict(), verify = False)

        if s.ok:
            if s.json()["errMsg"] == "":
                # self.msg = 200
                print("注册成功:", user.username)
            else:
                # self.msg = 201
                print("认证成功但用户已注册:", user.username)
        else:
            # self.msg = s.status_code
            print("认证失败:", user.username)

    @task(1)
    def login(self):
        persons, _ = generate_test_data()
        user = persons[random.randint(0, len(persons))]
        url = baseUrl + authApi

        s = self.client.post(url, data=user.get_login_dict(), verify = False)
        if s.status_code == 200:
            user.authorization = s.headers["Authorization"]
            print("登录成功:", user.username)
        elif s.status_code == 401:
            print("认证失败（用户不存在或密码错误）:", user.username)
        else:
            print("服务端错误")

salers = []
customers = []
coupons = []

def generate_test_data(num_of_sales, num_of_customers, num_of_coupons):
    num_of_coupons = min(5, num_of_coupons)
    for i in range(num_of_sales):
        salers.append(Sales(username="saler"+str(i), password="saler"+str(i)))
    for i in range(num_of_customers):
        customers.append(Customer(username="customer"+str(i), password="customer"+str(i)))
    for i in range(num_of_sales):
        for j in range(num_of_coupons):
            coupons.append(Coupon(name="saler"+str(i)+"_coupon"+str(j), amount=200, stock=100))
    # return salers, customers, coupons

def step_one(num_of_sales, num_of_customers, num_of_coupons):
    generate_test_data(num_of_sales, num_of_customers, num_of_coupons)
    session = requests.session()

    # 注册商家和普通用户
    for person in salers:
        TestRegistration(user=person, session=session)
    for person in customers:
        TestRegistration(user=person, session=session)

    # 模拟商家登录
    for person in salers:
        TestLogin(user=person, session=session)
    for person in customers:
        TestLogin(user=person, session=session)

    # 使用商家身份添加优惠券
    for i in range(num_of_sales):
        for j in range(num_of_coupons):
            TestAddCoupons(user=salers[i], coupon=coupons[j+i*num_of_coupons], session=session)

    # return salers, customers, coupons

def init():
    step_one(200, 1000, 3)

class TestCustomer(TaskSet):
    
#     @task(1)
#     def customer_login(self):
#         print('[salers] {} | [customers] {} | [coupons] {}'.format(len(salers), len(customers), len(coupons)))
#         c_idx = random.randint(0, len(customers)-1)
#         url = authApi
#         s = self.client.post(url,
#                             data=json.dumps(customers[c_idx].get_login_dict()),
#                             headers={"content-type": "application/json"})
#         if s.status_code == 204:
#             return
#         print('{} | {}'.format(url, s.json()))
#         if s.status_code == 204:
#             print("status-code: 204, empty message.")
#         elif s.status_code == 200:
#             customers[c_idx].authorization = s.headers["Authorization"]
#             print("User {} login".format(customers[c_idx].username))
#         else:
#             print("Error message:" , s.json()["errMsg"])

    # @task(1)
    # def get_coupons_info(self):
    #     print('[salers] {} | [customers] {} | [coupons] {}'.format(len(salers), len(customers), len(coupons)))
    #     searchee_select = random.randint(0, 1)
    #     searchee = salers[random.randint(0, len(salers) - 1)] if searchee_select == 1 else customers[random.randint(0, len(customers) - 1)]
    #     searcher_select = random.randint(0, 1)
    #     searcher = searchee
    #     # searcher = salers[random.randint(0, len(salers))] if searchee_select == 1 else customers[random.randint(0, len(customers))]
    #     # coupon = coupons[random.randint(0, len(coupons))]

    #     url = '/api/users/' + searchee.username + '/coupons'
    #     data = MultipartEncoder(fields={"page": "1"})
    #     headers = {
    #         "Authorization": searcher.authorization,
    #         'content-type': "application/json"
    #     }
    #     g = self.client.get(url, data=json.dumps({'page': '1'}), headers=headers, verify = False)
    #     # self.msg = g.status_code
    #     # print(g.text)
    #     if g.status_code == 204:
    #         return
    #     print('{} | {} | {}'.format(url, g.json(), searcher.username))
    #     if searchee.kind == "saler":
    #         if g.status_code == 200:
    #             if searcher.kind == "saler":
    #                 print("[Sales]" + searcher.username + " <===Coupons=== [Sales]" + searchee.username)
    #             elif searcher.kind == "customer":
    #                 print("[Customer]" + searcher.username + " <===Coupons=== [Sales]" + searchee.username)
    #         elif g.status_code == 204:
    #             print("查询商家剩余的优惠券信息，但查询结果为空")
    #         elif g.status_code == 401:
    #             print("查询商家: {}".format(searcher.username))
    #         elif g.status_code == 400:
    #             print("查询的用户不存在: {}".format(searchee.username))
    #         else:
    #             print("服务端错误")
    #     else:
    #         if g.status_code == 200:
    #             print("[Customer] " + searcher.username + " 查询已抢到的优惠券信息")
    #         elif g.status_code == 204:
    #             print("查询顾客自己已经抢到的优惠券信息，但查询结果为空")
    #         elif g.status_code == 401:
    #             print("试图访问其他顾客的优惠券列表，认证失败: {}".format(searcher.username))
    #         elif g.status_code == 400:
    #             print("查询的用户不存在: {}".format(searchee.username))
    #         else:
    #             print("服务端错误")

    @task(1)
    def get_coupons(self):
        print('[salers] {} | [customers] {} | [coupons] {}'.format(len(salers), len(customers), len(coupons)))
        sales_idx = random.randint(0, len(salers) - 1)
        sales = salers[sales_idx]
        customer = customers[random.randint(0, len(customers) - 1)]
        coupon = coupons[sales_idx * 3 + random.randint(0, 2)]

        url = '/api/users/' + sales.username + "/coupons/" + coupon.name
        headers = {
            "Authorization": customer.authorization,
            'content-type': "application/json"
        }
        g = self.client.patch(url, headers=headers, verify = False)
        # self.msg = g.status_code
        if g.status_code == 204:
            return
        print('{} | {}'.format(url, g.json()))
        if g.status_code == 201:
            print("[Customer]" + customer.username + " got [Coupon]"
                  + coupon.name + " from [Sales]" + sales.username)
        elif g.status_code == 204:
            print("[Customer]" + customer.username + " failed to get [Coupon]"
                  + coupon.name + " from [Sales]" + sales.username +
                  ", because he had one, or [Sales]" + sales.username + "don't have this coupon.")
        elif g.status_code == 401:
            print("[Customer] " + customer.username + " 认证失败 -- {}".format(g.json()))
        else:
            print("服务端错误 -- {}".format(g.json()))

class TestLocust(HttpLocust):
    init()
    # task_set = TestUser #压测任务名
    task_set = TestCustomer

    # min_wait = 3000 #最少等待时间
    # max_wait = 6000 #最长等待时间
    wait_time = between(3, 6)

if __name__ == '__main__':
    #locust -f parallel_scripts_2.py --host=http://localhost:20080
    pass