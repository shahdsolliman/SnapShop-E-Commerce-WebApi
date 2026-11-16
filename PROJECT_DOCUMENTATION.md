# 📘 التوثيق الشامل لمشروع SnapShop E-Commerce Web API

---

## 📑 فهرس المحتويات
1. [نظرة عامة على المشروع (Overview & Vision)](#1-نظرة-عامة-على-المشروع)
2. [المعمارية وأنماط التصميم (Architecture & Design Patterns)](#2-المعمارية-وأنماط-التصميم)
3. [التقنيات والمكتبات المستخدمة (Tech Stack & Packages)](#3-التقنيات-والمكتبات-المستخدمة)
4. [شرح تفصيلي لطبقات المشروع (Layer by Layer Breakdown)](#4-شرح-تفصيلي-لطبقات-المشروع)
5. [شرح الميزات الكاملة (Features Deep-Dive)](#5-شرح-الميزات-الكاملة)
6. [الأمان وإدارة الصلاحيات (Security & RBAC)](#6-الأمان-وإدارة-الصلاحيات)
7. [قواعد البيانات و مخطط الكيانات (Database & ERD)](#7-قواعد-البيانات-ومخطط-الكيانات)
8. [واجهات المستخدم المدمجة (Storefront & Dashboard)](#8-واجهات-المستخدم-المدمجة)
9. [الاختبارات الآلية (Unit Testing Suite)](#9-الاختبارات-الآلية)
10. [دليل التشغيل والنشر (Setup & Deployment)](#10-دليل-التشغيل-والنشر)

---

## 1. نظرة عامة على المشروع

**SnapShop** هو نظام تجارة إلكترونية متكامل (Full-Featured E-Commerce Solution) مصمم ومبني وفق أعلى المعايير الهندسية في عالم **.NET Backend**.

### فكرة المشروع العملية:
* **صاحب المحل (Shop Owner)**: يدخل يومياً على **لوحة التحكم (Admin Dashboard)**، يرفع صور البضاعة الجديدة التي وصلت محله اليوم مع تحديد السعر والماركة والوصف.
* **الزبائن (Customers)**: يفتحون المتجر من الموبايل أو الكمبيوتر، يشاهدون بضاعة اليوم بصورها الحية وأسعارها بالجنيه المصري، يضيفون للمشتريات (السلة محفوظة في **Redis**)، ويدخلون عنوان التوصيل لإتمام الطلب.
* **المحل**: يستقبل الطلب فورا في لوحة التحكم، ويقوم بتجهيز الشحنة وتحديث حالتها إلى (تم الشحن / تم التسليم).

---

## 2. المعمارية وأنماط التصميم

تم بناء النظام وفق معمارية **Clean Architecture (Onion Architecture)** لعزل منطق العمل عن قواعد البيانات وإطارات العمل الخارجية.

```
+-----------------------------------------------------------+
|                      SnapShop.API                         |
|   (Controllers, Middlewares, Dtos, Helpers, UI Static)    |
+-----------------------------+-----------------------------+
                              |
+-----------------------------v-----------------------------+
|                  SnapShop.Application                     |
|         (OrderService, PaymentService, TokenService)      |
+-----------------------------+-----------------------------+
                              |
+-----------------------------v-----------------------------+
|                      SnapShop.Core                        |
|       (Entities, Contracts/Interfaces, Specifications)    |
+-----------------------------^-----------------------------+
                              |
+-----------------------------+-----------------------------+
|                 SnapShop.Infrastructure                   |
|     (StoreContext, Repositories, Redis, Identity, Migrations)
+-----------------------------------------------------------+
```

### أنماط التصميم المطبقة (Design Patterns):

1. **Specification Pattern**:
   * عزل شروط الاستعلام (Filtering, Sorting, Pagination, Includes) في كلاسات منفصلة ومغلفة بدلاً من كتابة كود LINQ مكرر داخل الـ Controllers.
   * أمثلة: `ProductWithBrandAndCategorySpecifications`، `OrderSpecifications`.

2. **Repository Pattern**:
   * إنشاء واجهة تجريد عامة `IGenericRepository<T>` لإخفاء تفاصيل التعامل المباشر مع Entity Framework Core وتسهيل عمل الـ Mocking في الاختبارات.

3. **Unit of Work Pattern**:
   * تجميع كافة عمليات الحفظ والتعديل داخل Transaction واحدة عبر كلاس `UnitOfWork` و `CompleteAsync()` لضمان تكامل البيانات (Atomicity).

4. **Dependency Inversion Principle (DIP)**:
   * الطبقات العليا لا تعتمد على الطبقات السفلى، بل يعتمد كلاهما على الـ Interfaces الموجودة في `SnapShop.Core`.

5. **DTO Pattern (Data Transfer Objects)**:
   * منع إرجاع كائنات الـ Domain Entities مباشرة للعميل لحماية البيانات الحساسة وفصل واجهة الـ API عن هيكل قاعدة البيانات.

---

## 3. التقنيات والمكتبات المستخدمة

| التقنية / المكتبة | الإصدار | سبب الاستخدام ودورها في النظام |
| :--- | :--- | :--- |
| **ASP.NET Core Web API** | `6.0` | الإطار الأساسي لبناء RESTful Endpoints عالية الأداء. |
| **Entity Framework Core** | `6.0.36` | ORM للتعامل مع قاعدة بيانات SQL Server عبر Code-First. |
| **Microsoft SQL Server** | `2022` | قاعدة البيانات العلائقية الرئيسية لتخزين المنتجات والمستخدمين والطلبات. |
| **StackExchange.Redis** | `2.8.37` | تخزين سلة المشتريات (Basket) داخل In-Memory Cache لسرعة استجابة فائقة. |
| **ASP.NET Core Identity** | `6.0` | إدارة المستخدمين، تشفير كلمات المرور (PBKDF2)، والصلاحيات. |
| **JWT Bearer Token** | `6.0` | توثيق الدخول بدون Session (Stateless) مع تضمين الـ Roles داخل الـ Claims. |
| **AutoMapper** | `12.0.1` | التحويل التلقائي الآمن بين الـ Entities والـ DTOs مع Custom Resolvers. |
| **Stripe.net** | `41.x` | بوابة الدفع العالمية، إنشاء الـ PaymentIntents ومعالجة الـ Webhooks. |
| **Serilog** | `7.0.0` | تسجيل الأحداث والأخطاء المنظم (Structured Logging) في الكونسول والملفات. |
| **xUnit** | `2.8.x` | إطار العمل لتشغيل وكتابة اختبارات الوحدة (Unit Tests). |
| **Moq** | `4.20.70` | محاكاة الـ Repositories والـ Services لاختبار منطق العمل بمعزل عن الداتا بيز. |
| **FluentAssertions** | `6.12.0` | كتابة شروط الاختبارات بشكل واضح وسهل القراءة (`should().Be(...)`). |
| **Docker & Docker Compose** | `Latest` | تشغيل الـ API، قاعدة البيانات SQL Server، وسيرفر Redis في حاويات موحدة. |
| **Swashbuckle (Swagger)** | `6.5.0` | توثيق الـ API التفاعلي مع إمكانية تجربة الـ JWT Bearer مباشرة. |

---

## 4. شرح تفصيلي لطبقات المشروع

### 1. `SnapShop.Core` (طبقة النواة - Domain)
* **المسؤولية**: لا تعتمد على أي مكتبة أو إطار عمل خارجي.
* **المحتويات**:
  * **`Entities/`**: كلاسات الجداول الأساسية (`Product`, `ProductBrand`, `ProductCategory`, `CustomerBasket`, `BasketItem`).
  * **`Entities/OrderAggregate/`**: مجمع الطلب ومكوناته (`Order`, `OrderItem`, `ProductItemOrder`, `DeliveryMethod`, `Address`, `OrderState`).
  * **`Entities/Identity/`**: كائن مستخدم النظام `AppUser` والعنوان `Address`.
  * **`Repositories.Contract/`**: واجهات المستودعات `IGenericRepository<T>`، `IBasketRepository`، `IUnitOfWork`.
  * **`Services.Contract/`**: واجهات منطق العمل `IOrderService`، `IPaymentService`، `ITokenService`.
  * **`Specifications/`**: شروط الاستعلام الديناميكية `ISpecifications<T>`، `BaseSpecifications<T>`، `ProductSpecs/`، `OrderSpecs/`.

### 2. `SnapShop.Application` (طبقة التطبيق - Business Logic)
* **المسؤولية**: تنفيذ منطق العمل المعقد والتنسيق بين المستودعات وبوابات الدفع.
* **المحتويات**:
  * **`OrderService.cs`**:
    * جلب المنتجات من السيرفر للتأكد من السعر الحقيقي وحمايته من التلاعب في المتصفح.
    * حساب الإجمالي ومصاريف الشحن.
    * جلب طلبات الزبائن وتحديث حالات الشحن والتسليم.
  * **`PaymentService.cs`**: إنشاء وتحديث نية الدفع مع Stripe وحساب المبالغ سنت بسنت.
  * **`TokenService.cs`**: توليد رموز JWT المشفرة وحقن الـ Roles (Admin/Customer) والـ Claims.

### 3. `SnapShop.Infrastructure` (طبقة البنية التحتية - Data & External)
* **المسؤولية**: الاتصال بالخوادم، قواعد البيانات، والخدمات الخارجية.
* **المحتويات**:
  * **`Data/StoreContext.cs`**: سياق بيانات المتجر (Products, Orders, Delivery Methods).
  * **`Data/Configurations/`**: ضبط خصائص الأعمدة والعلاقات عبر Fluent API.
  * **`Data/StoreContextSeed.cs`**: التغذية التلقائية للماركات والفئات والبيانات الافتراضية.
  * **`Identity/AppIdentityDbContext.cs`**: سياق بيانات الـ Identity وتخزين المستخدمين والأدوار.
  * **`Identity/AppIdentityDbContextSeed.cs`**: زرع الأدوار وحسابات الأدمن والزبائن تلقائياً.
  * **`GenericRepository.cs`**: تطبيق الـ Generic Repository مع معالج الـ Specifications.
  * **`BasketRepository.cs`**: تطبيق سلة المشتريات باستخدام Redis Database مع TTL لمدة 30 يوماً.
  * **`UnitOfWork.cs`**: إدارة الـ DbContext وتوحيد الـ SaveChanges.

### 4. `SnapShop.API` (طبقة العرض والواجهة - Presentation)
* **المسؤولية**: استلام طلبات HTTP، التحقق من المدخلات، وتوجيه الاستجابة.
* **المحتويات**:
  * **`Controllers/`**: وحدات التحكم لجميع الميزات (`ProductsController`, `OrdersController`, `BasketController`, `AccountsController`, `PaymentsController`, `BuggyController`).
  * **`Dtos/`**: كائنات نقل البيانات لكل طلب واستجابة.
  * **`Errors/`**: استجابات موحدة للأخطاء (`ApiResponse`, `ApiExceptionResponse`, `ApiValidationErrorResponse`).
  * **`MiddleWares/`**: الميدل وير العام لاصطياد الـ Exceptions وحفظها عبر Serilog.
  * **`Helpers/`**: كلاسات AutoMapper ومحللات مسارات الصور (`ProductPictureUrlResolver`).
  * **`wwwroot/`**: واجهة متجر الزبائن بالكامل + لوحة تحكم صاحب المحل + مجلد تخزين الصور `uploads/`.

---

## 5. شرح الميزات الكاملة

### 1. بضاعة اليوم ورفع الصور (Daily Product Upload)
* صاحب المحل يرفع صور المنتجات اليومية من الموبايل أو الكمبيوتر بصيغة `multipart/form-data`.
* السيرفر يتحقق من:
  1. امتداد الملف (`jpg`, `jpeg`, `png`, `webp`, `gif`).
  2. الحجم لا يتجاوز 5 ميجابايت.
  3. تغيير اسم الملف لـ `Guid` عشوائي لمنع استبدال ملفات أخرى.
* حفظ الملف في مجلد `wwwroot/uploads` وتخزين المسار النسبي في قاعدة البيانات.
* حذف الصورة فيزيائياً من القرص عند حذف المنتج لعدم استهلاك مساحة السيرفر.

### 2. سلة التسوق فائقة السرعة مع Redis (Distributed Basket)
* لا تتطلب تسجيل دخول مسبق؛ يتم إنشاء `BasketId` عشوائي في المتصفح.
* تخزين محتويات السلة في سيرفر **Redis** كـ JSON مضغوط مع مدة صلاحية (TTL).
* حفظ السلة للزبون حتى لو أغلق المتصفح أو أعاد فتحه لاحقاً.

### 3. تأمين الطلبات وحساب الأسعار (Secure Order Creation)
* **منع التلاعب بالأسعار**: حتى لو قام زبون بتعديل سعر المنتج داخل المتصفح أو عبر Postman، فإن الـ `OrderService` تتجاهل سعر الكلاينت تماماً، وتقوم بقراءة السعر الحقيقي المخزن في جدول الـ Products بالسيرفر.
* ربط الطلب بطريقة التوصيل وعنوان الشحن.
* إفراغ سلة الـ Redis فور إتمام الطلب بنجاح.

### 4. إدارة حالات الطلب من الداشبورد (Order Lifecycle)
* كل طلب جديد يدخل بحالة `Pending` (قيد الانتظار).
* صاحب المحل يستطيع بنقرة واحدة من الداشبورد تغيير الحالة:
  * `Pending` (قيد الانتظار) ➡️ `PaymentReceived` (تم الدفع) ➡️ `Shipped` (خرج للشحن) ➡️ `Delivered` (تم التسليم للزبون).

---

## 6. الأمان وإدارة الصلاحيات

* **تشفير كلمات المرور**: عبر خوارزمية التجزئة المدمجة في ASP.NET Identity (PBKDF2 with HMAC-SHA256).
* **JWT Authentication**:
  * توليد الرمز بمفتاح سري مشفر `SymmetricSecurityKey`.
  * التحقق من الـ Issuer والـ Audience والـ Lifetime.
* **Role-Based Authorization (RBAC)**:
  * `[Authorize(Roles = "Admin")]`: خاص برفع المنتجات، حذفها، استعراض كل الطلبات، وتعديل حالاتها.
  * `[Authorize]`: خاص بإجراء الطلبات واستعراض طلبات الزبون الشخصية.
  * `[AllowAnonymous]`: لتصفح المنتجات وطرق التوصيل وتسجيل الدخول.

---

## 7. قواعد البيانات ومخطط الكيانات

يتعامل النظام مع سياقين لقاعدة البيانات (Two DbContexts):

1. **`StoreContext`**:
   * `Products` (المنتجات) ⬅️ مرتبط بـ `ProductBrands` و `ProductCategories`.
   * `Orders` (الطلبات) ⬅️ مرتبط بـ `OrderItems` و `DeliveryMethods`.
   * `OrderItems` يحتوي على `ProductItemOrder` كـ Value Object.

2. **`AppIdentityDbContext`**:
   * `AspNetUsers` (المستخدمين) ⬅️ مرتبط بـ `Address` كعلاقة 1 إلى 1.
   * `AspNetRoles` (الأدوار: Admin و Customer).
   * `AspNetUserRoles` (ربط المستخدمين بأدوارهم).

---

## 8. واجهات المستخدم المدمجة

المشروع يحتوي على واجهتين كاملتين داخل مجلد `wwwroot/` تعملان بدون أي إطارات خارجية ثقيلة (Vanilla HTML/CSS/JS):

### 🛍️ واجهة الزبائن (`/index.html`):
* الرابط: **`https://localhost:7205/`**
* تصميم عصري (Dark Mode) وخط Cairo.
* بحث فوري، فلترة بالماركة والفئة، وترتيب حسب السعر.
* سلة مشتريات عائمة تنزلق من الجانب (Slide-over Drawer) متصلة بـ Redis.
* شاشة Checkout لإدخال الاسم، الهاتف، العنوان، واختيار التوصيل.
* شاشة احتفال برقم الطلب بعد التأكيد.

### ⚙️ لوحة الإدارة (`/admin/index.html`):
* الرابط: **`https://localhost:7205/admin/index.html`**
* تسجيل دخول مؤمن بصلاحية `Admin` فقط.
* إحصائيات مالية فورية (إجمالي المنتجات، إجمالي المبيعات، الطلبات المعلقة).
* تبويب بضاعة اليوم: إضافة بضاعة جديدة مع رفع الصورة وحذف المنتجات المباعة.
* تبويب طلبات الزبائن: جدول بكل الطلبات وتحديث فوري للحالة.

---

## 9. الاختبارات الآلية

مشروع [`SnapShop.UnitTests`](file:///e:/SnapShop/SnapShop-E-Commerce-WebApi/SnapShop.UnitTests/) يضم **17 اختبار وحدة** تغطي:

* اختبارات `OrderService` (حساب الأسعار، السلة الفارغة، غياب طريقة التوصيل، تحديث الحالات).
* اختبارات `Specifications` (التحقق من صحة جمل الـ LINQ، الفلترة، والترتيب).
* اختبارات `Entities` (حساب إجمالي الفاتورة بدقة وحالات السلة).

لتشغيل الاختبارات:
```powershell
dotnet test
```
النتيجة: **`Passed: 17, Failed: 0, Total: 17`**.

---

## 10. دليل التشغيل والنشر

### التشغيل المحلي:
```powershell
# 1. استعادة الحزم والبناء
dotnet restore
dotnet build

# 2. تشغيل السيرفر
dotnet run --project SnapShop.API
```

### التشغيل عبر Docker Compose:
```powershell
docker compose up -d
```
يقوم بتشغيل:
* السيرفر على البورت `5000`
* SQL Server على البورت `1433`
* Redis على البورت `6379`

### بيانات الدخول الافتراضية:
* **الأدمن**: `admin@snapshop.com` / `Admin$123`
* **الزبون**: `demo@snapshop.com` / `Pa$$w0rd`
