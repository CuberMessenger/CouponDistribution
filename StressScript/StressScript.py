import requests
import time
from requests_toolbelt import MultipartEncoder
import json

baseUrl = "https://localhost:44317"
userApi = "/api/users"
authApi = "/api/auth"
couponsApi = "/coupons"

requests.packages.urllib3.disable_warnings()

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
        s = self.response.post(self.url, json=self.user.get_registration_dict(), verify = False)

        if s.ok:
            if s.json()["errMsg"] == "":
                self.msg = 200
                print("注册成功:", self.user.username)
            else:
                self.msg = 201
                print("认证成功但用户已注册:", self.user.username)
        else:
            self.msg = s.status_code
            print("认证失败:", self.user.username, "\t", s.status_code, "\t", s.json()["errMsg"])


class TestLogin(Test):
    def __init__(self, user, session):
        Test.__init__(self, session)
        self.user = user
        self.url += authApi
        self.login()

    def login(self):
        s = self.response.post(self.url, json=self.user.get_login_dict(), verify = False)
        self.msg = s.status_code
        if s.status_code == 200:
            self.user.authorization = s.headers["Authorization"]
            print("登录成功:", self.user.username)
        elif s.status_code == 401:
            print("认证失败（用户不存在或密码错误）:", self.user.username)
        else:
            print("服务端错误\t", s.status_code)


class TestAddCoupons(Test):
    def __init__(self, user, coupon, session):
        Test.__init__(self, session)
        self.user = user
        self.coupon = coupon
        self.url += userApi + "/"
        self.generate_new_coupons()

    def generate_new_coupons(self):
        if self.user.kind == "saler":
            url = self.url + self.user.username + couponsApi
            s = self.response.post(url, json=self.coupon.get_coupons_dict(),
                                   headers={"Authorization": self.user.authorization}, verify = False)
            # self.coupon.sales = self.user.username
            self.msg = s.status_code
            if s.status_code == 201:
                print(self.user.username + " 新建了 " + str(self.coupon.amount) + " 张 " + self.coupon.name + " Coupons")
            elif s.status_code == 401:
                print("认证失败:", self.user.username)
            elif s.status_code == 400:
                print("创建用户原因导致的创建失败或其他问题:", self.user.username, "\t", s.json()["errMsg"])
            else:
                print("服务端错误\t", s.status_code, "\t", s.json()["errMsg"])
        else:
            print("非商家，不能新建Coupons")


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
            'Content-Type': "application/json"
            #'Content-Type': data.content_type
        }
        g = self.response.get(url, json=json.dumps({"page": "1"}), headers=headers, verify = False)
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
                print("查询的用户不存在:", self.searchee.username, "\t", g.json()["errMsg"])
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
                print("查询的用户不存在:", self.searchee.username, "\t", g.json()["errMsg"])
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
            print("服务端错误\t", g.status_code, "\t", g.json()["errMsg"])



def test():

    # person
    s1 = Sales("boss", "123")
    s2 = Sales("cdl", "123")
    cu1 = Customer("dzf", "123")
    cu2 = Customer("yzh", "123")
    cu3 = Customer("dzf", "232")
    s3 = Sales("dalao", "123")

    #coupons
    coupon1 = Coupon("A", "200", "1", "A!")
    coupon2 = Coupon("B", "200", "1", "B!")
    coupon3 = Coupon("C", "200", "1", "C!")
    coupon4 = Coupon("D", "200", "1", "D!")

    # new a request session
    session = requests.session()

    start = time.time()

    # register test
    _ = TestRegistration(s1, session)
    _ = TestRegistration(s2, session)
    _ = TestRegistration(s3, session)
    _ = TestRegistration(cu1, session)
    _ = TestRegistration(cu2, session)
    _ = TestRegistration(cu3, session)
    print("########################################")

    # login test
    _ = TestLogin(s1, session)
    _ = TestLogin(s2, session)
    _ = TestLogin(s3, session)
    _ = TestLogin(cu1, session)  # 登录成功: dzf
    _ = TestLogin(cu2, session)
    _ = TestLogin(cu3, session)  # 认证失败（用户不存在或密码错误）: dzf
    print("########################################")

    end = time.time()
    print("Register and Login cost: %.3f" % (end - start))
    start = time.time()

    # add coupons test
    _ = TestAddCoupons(s1, coupon1, session)
    _ = TestAddCoupons(s1, coupon2, session)
    _ = TestAddCoupons(s2, coupon3, session)
    _ = TestAddCoupons(cu1, coupon4, session)  # 非商家，不能新建Coupons
    print("########################################")

    # get coupons test
    _ = TestCustomerGetCoupons(customer=cu1, sales=s1, coupon=coupon1, session=session)
    _ = TestCustomerGetCoupons(customer=cu1, sales=s1, coupon=coupon1, session=session)
    print("########################################")

    # get coupons info test
    _ = TestGetCouponsInfo(searcher=s1, searchee=s1, session=session)  # [Sales]boss <===Coupons=== [Sales]boss
    _ = TestGetCouponsInfo(searcher=cu1, searchee=s1, session=session)  # [Customer]dzf <===Coupons=== [Sales]boss
    _ = TestGetCouponsInfo(searcher=cu1, searchee=cu1, session=session)  # [Customer]dzf查询已抢到的优惠券信息
    _ = TestGetCouponsInfo(searcher=cu1, searchee=cu2, session=session)  # 试图访问其他顾客的优惠券列表，认证失败: dzf
    _ = TestGetCouponsInfo(searcher=s3, searchee=s3, session=session)  # 查询商家剩余的优惠券信息，但查询结果为空

    print("########################################")
    end = time.time()
    print("Other test cost: %.3f" % (end - start))

    #Add-Migration BuildDatabase
    #Update-Database

    session.close()


def test_1000():
    
    sales_num = 1
    customer_num = 1
    coupon_num = 1
    
    for t in range(0, 10):
    
        sales_list = []
        customer_list = []
        coupon_list = []
        
        for i in range(0, 10):
            
            sales = Sales("sales"+str(sales_num), "123")
            sales_list.append(sales)
            sales_num += 1
            
            customer = Customer("customer"+str(customer_num), "123")
            customer_list.append(customer)
            customer_num += 1
            
            for j in range(0, 10):
                
                coupon = Coupon("coupon"+str(coupon_num), "10", "1", "A!")
                coupon_list.append(coupon)
                coupon_num += 1
                
        session = requests.session()
        
        start = time.time()
        
        for i in range(0, 10):
            
            _ = TestRegistration(sales_list[i], session)
            _ = TestRegistration(customer_list[i], session)
            
            _ = TestLogin(sales_list[i], session)
            _ = TestLogin(customer_list[i], session)
            
        end = time.time()
        print("Register and Login cost: %.3f" % (end - start))
        
        start = time.time()
        
        for i in range(0, 10):
            for j in range(0, 10):
                _ = TestAddCoupons(sales_list[i], coupon_list[i*10+j], session)
        
        for i in range(0, 10):
            for j in range(0, 10):
                _ = TestCustomerGetCoupons(customer_list[i], sales_list[j], coupon_list[j*10+i], session)
        
        for i in range(0, 10):
            
            _ = TestGetCouponsInfo(searcher=sales_list[i], searchee=sales_list[i], session=session)
            _ = TestGetCouponsInfo(searcher=customer_list[i], searchee=customer_list[i], session=session)
            _ = TestGetCouponsInfo(searcher=customer_list[i], searchee=customer_list[9-i], session=session)
            
            for j in range(0, 10):
                
                _ = TestGetCouponsInfo(searcher=customer_list[i], searchee=sales_list[j], session=session)
        
        end = time.time()
        print("Other test cost: %.3f" % (end - start))
        
        session.close()

if __name__ == '__main__':
    #test()
    test_1000()
