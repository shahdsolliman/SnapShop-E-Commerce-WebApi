# 📮 SnapShop Web API — دليل الاختبار الشامل عبر Postman

هذا الدليل يوضح خطوة بخطوة كيفية اختبار جميع الـ Endpoints الخاصة بمشروع **SnapShop** عبر برنامج **Postman** بالترتيب المنطقي للعمليات (من تسجيل الدخول ورفع المنتجات إلى السلة والدفع والطلب).

---

## ⚙️ 1. إعداد بيئة العمل (Postman Environment)

قبل البدء، أنشئ **Environment** في Postman وضع المتغيرات التالية:

| المتغير (Variable) | القيمة الافتراضية (Initial Value) | الوصف |
| :--- | :--- | :--- |
| `baseUrl` | `https://localhost:7205` | رابط السيرفر المحلي للـ API |
| `adminToken` | *(يتم ملؤه تلقائياً أو يدوياً بعد تسجيل الدخول)* | رمز JWT للأدمن |
| `customerToken` | *(يتم ملؤه تلقائياً أو يدوياً بعد تسجيل الدخول)* | رمز JWT للزبون |
| `basketId` | `basket_postman_test` | معرّف سلة التسوق في Redis |

---

## 🔐 2. الحسابات وتوثيق الهوية (Identity & Authentication)

### 2.1 تسجيل دخول الأدمن (Admin Login)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/Accounts/login`
* **Headers**: `Content-Type: application/json`
* **Body** (raw JSON):
  ```json
  {
    "email": "admin@snapshop.com",
    "password": "Admin$123"
  }
  ```
* **الاستجابة المتوقعة (200 OK)**:
  ```json
  {
    "displayName": "Admin User",
    "email": "admin@snapshop.com",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```
> 💡 **ملاحظة**: انسخ قيمة الـ `token` وضعها في متغير `{{adminToken}}`.

---

### 2.2 تسجيل دخول الزبون (Customer Login)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/Accounts/login`
* **Headers**: `Content-Type: application/json`
* **Body** (raw JSON):
  ```json
  {
    "email": "demo@snapshop.com",
    "password": "Pa$$w0rd"
  }
  ```
> 💡 **ملاحظة**: انسخ قيمة الـ `token` وضعها في متغير `{{customerToken}}`.

---

### 2.3 تسجيل حساب زبون جديد (Customer Registration)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/Accounts/register`
* **Headers**: `Content-Type: application/json`
* **Body** (raw JSON):
  ```json
  {
    "displayName": "Mohamed Ali",
    "email": "mohamed@test.com",
    "phoneNumber": "01012345678",
    "password": "Password$123"
  }
  ```
* **الاستجابة المتوقعة**: `201 Created` مع الـ Token.

---

### 2.4 فحص هل البريد مسجل مسبقاً (Check Email Exists)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/Accounts/emailexists?email=admin@snapshop.com`
* **الاستجابة المتوقعة**: `true` أو `false`.

---

### 2.5 جلب بيانات المستخدم الحالي (Get Current User)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/Accounts`
* **Headers**: 
  * `Authorization`: `Bearer {{customerToken}}`
* **الاستجابة المتوقعة (200 OK)**: بيانات المستخدم مع Token مجدد.

---

## 📦 3. إدارة المنتجات والميديا (Products API)

### 3.1 جلب كل المنتجات مع الفلترة والصفحات (Get Products)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/products?pageIndex=1&pageSize=10&sort=priceAsc`
* **Parameters اختيارية**:
  * `brandId`: رقم العلامة التجارية (مثال: `1`)
  * `categoryId`: رقم الفئة (مثال: `2`)
  * `sort`: `priceAsc`, `priceDesc`, `name`
  * `search`: كلمة البحث بالاسم (مثال: `Nike`)
* **الاستجابة المتوقعة (200 OK)**:
  ```json
  {
    "pageIndex": 1,
    "pageSize": 10,
    "count": 18,
    "data": [
      {
        "id": 1,
        "name": "حذاء نايكي رياضي",
        "description": "حذاء جري عالي الجودة ومريح",
        "price": 299.99,
        "pictureUrl": "/uploads/xyz.jpg",
        "productBrand": "Nike",
        "productCategory": "أحذية وحقائب"
      }
    ]
  }
  ```

---

### 3.2 جلب منتج محدد بالـ ID (Get Product By ID)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/products/1`
* **الاستجابة المتوقعة (200 OK)**: تفاصيل المنتج كاملة.

---

### 3.3 جلب جميع الماركات (Get Brands)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/products/brands`

---

### 3.4 جلب جميع الفئات (Get Categories)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/products/categories`

---

### 3.5 رفع وإضافة منتج جديد مع صورة (Create Product - Admin Only)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/products`
* **Headers**: 
  * `Authorization`: `Bearer {{adminToken}}`
* **Body**: اختر **form-data**:
  * `Name` (Text): `ساعة ذكية Apple Watch`
  * `Description` (Text): `أحدث إصدار مع شاشة مقاومة للخدش`
  * `Price` (Text): `1250.00`
  * `BrandId` (Text): `4` (ماركة Apple)
  * `CategoryId` (Text): `2` (فئة إلكترونيات)
  * `Image` (File): *اختر أي ملف صورة JPG أو PNG من جهازك*
* **الاستجابة المتوقعة**: `201 Created` مع رابط الصورة المرفوعة في `/uploads/...`.

---

### 3.6 حذف منتج وصورته من السيرفر (Delete Product - Admin Only)
* **Method**: `DELETE`
* **URL**: `{{baseUrl}}/api/products/1`
* **Headers**: 
  * `Authorization`: `Bearer {{adminToken}}`
* **الاستجابة المتوقعة**: `204 No Content` (وحذف الصورة من مجلد `wwwroot/uploads`).

---

## 🛒 4. سلة التسوق و Redis (Basket API)

### 4.1 إضافة وتحديث سلة المشتريات (Create/Update Basket)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/basket`
* **Headers**: `Content-Type: application/json`
* **Body** (raw JSON):
  ```json
  {
    "id": "{{basketId}}",
    "items": [
      {
        "id": 1,
        "name": "حذاء نايكي رياضي",
        "description": "حذاء جري عالي الجودة ومريح",
        "price": 299.99,
        "quantity": 2,
        "pictureUrl": "/uploads/xyz.jpg",
        "brand": "Nike",
        "category": "أحذية وحقائب"
      }
    ]
  }
  ```
* **الاستجابة المتوقعة (200 OK)**: السلة المحفوظة في ذاكرة Redis مع مدة انتهاء صلاحية (TTL).

---

### 4.2 استرجاع سلة المشتريات (Get Basket)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/basket?id={{basketId}}`
* **الاستجابة المتوقعة (200 OK)**: محتويات السلة من سيرفر Redis.

---

### 4.3 حذف سلة المشتريات (Delete Basket)
* **Method**: `DELETE`
* **URL**: `{{baseUrl}}/api/basket?id={{basketId}}`
* **الاستجابة المتوقعة (200 OK)**: `true`.

---

## 🚚 5. طرق التوصيل والطلبات (Orders API)

### 5.1 جلب طرق التوصيل المتاحة (Get Delivery Methods)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/orders/deliveryMethods`
* **الاستجابة المتوقعة (200 OK)**:
  ```json
  [
    {
      "id": 1,
      "shortName": "توصيل عادي",
      "deliveryTime": "2-3 أيام",
      "description": "توصيل اقتصادي لباب المنزل",
      "cost": 30.00
    },
    {
      "id": 2,
      "shortName": "توصيل سريع (Express)",
      "deliveryTime": "خلال 24 ساعة",
      "description": "أسرع توصيل للمدن الرئيسية",
      "cost": 60.00
    }
  ]
  ```

---

### 5.2 إنشاء طلب جديد (Create Order)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/orders`
* **Headers**: 
  * `Authorization`: `Bearer {{customerToken}}`
  * `Content-Type`: `application/json`
* **Body** (raw JSON):
  ```json
  {
    "basketId": "{{basketId}}",
    "deliveryMethodId": 1,
    "shippingAddress": {
      "firstName": "أحمد",
      "lastName": "السيد",
      "street": "شارع النصر، عمارة 12، شقة 5",
      "city": "القاهرة",
      "country": "مصر"
    }
  }
  ```
* **الاستجابة المتوقعة (201 Created)**:
  ```json
  {
    "id": 1,
    "buyerEmail": "demo@snapshop.com",
    "orderDate": "2026-10-27T02:00:00+03:00",
    "shipToAddress": {
      "firstName": "أحمد",
      "lastName": "السيد",
      "street": "شارع النصر، عمارة 12، شقة 5",
      "city": "القاهرة",
      "country": "مصر"
    },
    "deliveryMethod": "توصيل عادي",
    "shippingPrice": 30.00,
    "orderItems": [
      {
        "productId": 1,
        "productName": "حذاء نايكي رياضي",
        "pictureUrl": "/uploads/xyz.jpg",
        "price": 299.99,
        "quantity": 2
      }
    ],
    "subTotal": 599.98,
    "total": 629.98,
    "status": "Pending"
  }
  ```

---

### 5.3 جلب طلبات الزبون الحالي (Get Orders For Customer)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/orders`
* **Headers**: `Authorization: Bearer {{customerToken}}`
* **الاستجابة المتوقعة (200 OK)**: قائمة بجميع طلبات هذا الزبون مرتبة تنازلياً حسب التاريخ.

---

### 5.4 جلب تفاصيل طلب محدد بالـ ID (Get Order By ID)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/orders/1`
* **Headers**: `Authorization: Bearer {{customerToken}}`

---

### 5.5 جلب كافة طلبات الزبائن (Get All Orders - Admin Only)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/api/orders/all`
* **Headers**: `Authorization: Bearer {{adminToken}}`
* **الاستجابة المتوقعة (200 OK)**: قائمة بكل طلبات الزبائن في النظام لإدارتها من الداشبورد.

---

### 5.6 تحديث حالة الطلب (Update Order Status - Admin Only)
* **Method**: `PUT`
* **URL**: `{{baseUrl}}/api/orders/1/status?status=Shipped`
* **Headers**: `Authorization: Bearer {{adminToken}}`
* **خيارات الحالة (status)**:
  * `Pending`: قيد الانتظار
  * `PaymentReceived`: تم استلام الدفع
  * `Shipped`: خرج للشحن والتوصيل
  * `Delivered`: تم التسليم للزبون
  * `PaymentFailed`: فشل الدفع
* **الاستجابة المتوقعة (200 OK)**: الطلب بعد تحديث حالته.

---

## 💳 6. الدفع الإلكتروني عبر Stripe (Payments API)

### 6.1 إنشاء أو تحديث نية الدفع (Create Or Update Payment Intent)
* **Method**: `POST`
* **URL**: `{{baseUrl}}/api/payments/{{basketId}}`
* **Headers**: `Authorization: Bearer {{customerToken}}`
* **الاستجابة المتوقعة (200 OK)**: يرجع السلة مع `clientSecret` و `paymentIntentId` للربط مع Stripe Elements.

---

## 🩺 7. فحص صحة النظام (Health Checks)

### 7.1 فحص السيرفر والاتصال (Health Check Endpoint)
* **Method**: `GET`
* **URL**: `{{baseUrl}}/health`
* **الاستجابة المتوقعة (200 OK)**: `Healthy`.
