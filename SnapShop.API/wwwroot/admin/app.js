/* ══════════════════════════════════════════════════════
   SnapShop Admin Dashboard — app.js
   Full Store Owner experience:
   - Product uploads with optional price & photo preview
   - Live Order management (Pending -> Accepted -> Shipped -> Delivered -> Cancelled)
   - WhatsApp customer direct outreach
   - Invoice modal & printable view
   - Filter chips & search
   ══════════════════════════════════════════════════════ */

(() => {
  'use strict';

  const API_BASE = window.location.origin + '/api';

  // ── DOM References ──
  const loginSection           = document.getElementById('loginSection');
  const dashboardSection       = document.getElementById('dashboardSection');
  const loginForm              = document.getElementById('loginForm');
  const loginSubmitBtn         = document.getElementById('loginSubmitBtn');
  const fillDemoCredentialsBtn = document.getElementById('fillDemoCredentialsBtn');
  const logoutBtn              = document.getElementById('logoutBtn');
  const adminGreeting          = document.getElementById('adminGreeting');

  // Stats
  const statsProducts = document.getElementById('statsProducts');
  const statsOrders   = document.getElementById('statsOrders');
  const statsPending  = document.getElementById('statsPending');
  const statsShipped  = document.getElementById('statsShipped');
  const statsSales    = document.getElementById('statsSales');
  const pendingOrdersBadge = document.getElementById('pendingOrdersBadge');

  // Tabs
  const tabProductsBtn     = document.getElementById('tabProductsBtn');
  const tabOrdersBtn       = document.getElementById('tabOrdersBtn');
  const tabProductsContent = document.getElementById('tabProductsContent');
  const tabOrdersContent   = document.getElementById('tabOrdersContent');

  // Products UI
  const productForm        = document.getElementById('productForm');
  const submitProductBtn   = document.getElementById('submitProduct');
  const refreshProductsBtn = document.getElementById('refreshProductsBtn');
  const productsBody       = document.getElementById('productsBody');
  const brandSelect        = document.getElementById('brandId');
  const categorySelect     = document.getElementById('categoryId');
  const imageInput         = document.getElementById('image');
  const dropZone           = document.getElementById('dropZone');
  const dropZonePrompt     = document.getElementById('dropZonePrompt');
  const imagePreviewWrap   = document.getElementById('imagePreviewWrap');
  const imagePreview       = document.getElementById('imagePreview');
  const btnChangeImage     = document.getElementById('btnChangeImage');

  // Orders UI
  const ordersBody       = document.getElementById('ordersBody');
  const refreshOrdersBtn = document.getElementById('refreshOrdersBtn');
  const orderSearchInput = document.getElementById('orderSearchInput');
  const orderFilterChips = document.getElementById('orderFilterChips');
  const countAll         = document.getElementById('countAll');
  const countPending     = document.getElementById('countPending');
  const countAccepted    = document.getElementById('countAccepted');
  const countShipped     = document.getElementById('countShipped');
  const countDelivered   = document.getElementById('countDelivered');
  const countCancelled   = document.getElementById('countCancelled');

  // Modal & Lightbox
  const orderDetailModal     = document.getElementById('orderDetailModal');
  const modalOrderTitle      = document.getElementById('modalOrderTitle');
  const modalOrderBody       = document.getElementById('modalOrderBody');
  const closeDetailModalBtn  = document.getElementById('closeDetailModalBtn');
  const closeDetailModalBtn2 = document.getElementById('closeDetailModalBtn2');
  const lightboxModal        = document.getElementById('lightboxModal');
  const lightboxImg          = document.getElementById('lightboxImg');
  const lightboxClose        = document.getElementById('lightboxClose');

  // State
  let allOrders = [];
  let currentOrderFilter = 'all';
  let orderSearchQuery = '';

  // ── Toasts ──
  const toastContainer = document.getElementById('toastContainer');
  function showToast(message, type = 'success') {
    const t = document.createElement('div');
    t.className = `toast ${type}`;
    const icon = type === 'success' ? '✅' : (type === 'error' ? '⚠️' : 'ℹ️');
    t.innerHTML = `<span>${icon}</span> <span>${message}</span>`;
    toastContainer.appendChild(t);
    setTimeout(() => {
      t.style.opacity = '0';
      t.style.transform = 'translateY(-10px)';
      setTimeout(() => t.remove(), 400);
    }, 4000);
  }

  // ── Token Management ──
  function getToken() {
    return sessionStorage.getItem('snapshop_admin_token') || sessionStorage.getItem('snapshop_token');
  }
  function setToken(token) {
    sessionStorage.setItem('snapshop_admin_token', token);
    sessionStorage.setItem('snapshop_token', token);
  }
  function clearToken() {
    sessionStorage.removeItem('snapshop_admin_token');
    sessionStorage.removeItem('snapshop_token');
    sessionStorage.removeItem('snapshop_user');
  }

  function authHeaders() {
    return {
      'Authorization': `Bearer ${getToken()}`
    };
  }

  function parseJwt(token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(decodeURIComponent(
        atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
      ));
    } catch { return null; }
  }

  function isTokenValid() {
    const token = getToken();
    if (!token) return false;
    const payload = parseJwt(token);
    if (!payload) return false;
    if (payload.exp && Date.now() / 1000 > payload.exp) {
      clearToken();
      return false;
    }
    const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    const roleArr = Array.isArray(roles) ? roles : [roles];
    return roleArr.includes('Admin');
  }

  function showDashboard() {
    loginSection.classList.add('hidden');
    dashboardSection.classList.remove('hidden');
    const user = sessionStorage.getItem('snapshop_user') || 'صاحب المحل';
    if (adminGreeting) adminGreeting.textContent = `مرحباً بك، ${user} 👋`;
  }

  function showLogin() {
    dashboardSection.classList.add('hidden');
    loginSection.classList.remove('hidden');
  }

  // ══════════════════════════════════
  //  TABS
  // ══════════════════════════════════
  tabProductsBtn.addEventListener('click', () => {
    tabProductsBtn.classList.add('active');
    tabOrdersBtn.classList.remove('active');
    tabProductsContent.classList.remove('hidden');
    tabOrdersContent.classList.add('hidden');
  });

  tabOrdersBtn.addEventListener('click', () => {
    tabOrdersBtn.classList.add('active');
    tabProductsBtn.classList.remove('active');
    tabOrdersContent.classList.remove('hidden');
    tabProductsContent.classList.add('hidden');
    loadOrders();
  });

  // ══════════════════════════════════
  //  LOGIN & DEMO FILL
  // ══════════════════════════════════
  if (fillDemoCredentialsBtn) {
    fillDemoCredentialsBtn.addEventListener('click', () => {
      document.getElementById('email').value = 'admin@snapshop.com';
      document.getElementById('password').value = 'Admin$123';
      showToast('تم إدخال بيانات المسؤول التجريبية', 'info');
    });
  }

  loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;
    const btn = loginSubmitBtn;
    btn.disabled = true;
    btn.innerHTML = '<span>جاري التحقق والدخول...</span> ⏳';

    try {
      const res = await fetch(`${API_BASE}/Accounts/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || 'بيانات الدخول غير صحيحة. يرجى التأكد من البريد وكلمة المرور.');
      }

      const data = await res.json();
      setToken(data.token);
      sessionStorage.setItem('snapshop_user', data.displayName || 'صاحب المحل');

      if (!isTokenValid()) {
        clearToken();
        throw new Error('هذا الحساب ليس لديه صلاحيات المسؤول (Admin). يرجى استخدام حساب صاحب المحل.');
      }

      showToast(`مرحباً بك يا ${data.displayName || 'صاحب المحل'} 👋`, 'success');
      showDashboard();
      await loadDashboardData();
    } catch (err) {
      showToast(err.message, 'error');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<span>تسجيل الدخول</span> 🔐';
    }
  });

  logoutBtn.addEventListener('click', () => {
    if (!confirm('هل تريد تسجيل الخروج من لوحة الإدارة؟')) return;
    clearToken();
    showLogin();
    showToast('تم تسجيل الخروج بنجاح');
  });

  // ══════════════════════════════════
  //  IMAGE UPLOAD & DRAG/DROP
  // ══════════════════════════════════
  function handleFile(file) {
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      showToast('يرجى اختيار ملف صورة صالح (JPG, PNG, WebP)', 'error');
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      imagePreview.src = e.target.result;
      dropZonePrompt.style.display = 'none';
      imagePreviewWrap.style.display = 'block';
    };
    reader.readAsDataURL(file);
  }

  imageInput.addEventListener('change', () => {
    if (imageInput.files && imageInput.files[0]) {
      handleFile(imageInput.files[0]);
    }
  });

  if (btnChangeImage) {
    btnChangeImage.addEventListener('click', (e) => {
      e.stopPropagation();
      imageInput.value = '';
      imagePreviewWrap.style.display = 'none';
      dropZonePrompt.style.display = 'block';
      imageInput.click();
    });
  }

  ['dragenter', 'dragover'].forEach(eventName => {
    dropZone.addEventListener(eventName, (e) => {
      e.preventDefault();
      e.stopPropagation();
      dropZone.classList.add('dragover');
    }, false);
  });

  ['dragleave', 'drop'].forEach(eventName => {
    dropZone.addEventListener(eventName, (e) => {
      e.preventDefault();
      e.stopPropagation();
      dropZone.classList.remove('dragover');
    }, false);
  });

  dropZone.addEventListener('drop', (e) => {
    const dt = e.dataTransfer;
    const files = dt.files;
    if (files && files.length > 0) {
      imageInput.files = files;
      handleFile(files[0]);
    }
  });

  // ══════════════════════════════════
  //  LOAD BRANDS & CATEGORIES
  // ══════════════════════════════════
  async function loadBrandsAndCategories() {
    try {
      const [brandsRes, catsRes] = await Promise.all([
        fetch(`${API_BASE}/Products/brands`),
        fetch(`${API_BASE}/Products/categories`)
      ]);
      const brands = await brandsRes.json();
      const categories = await catsRes.json();

      if (brandSelect) {
        brandSelect.innerHTML = '<option value="">(تلقائي - اختيار عام)</option>' +
          brands.map(b => `<option value="${b.id}">${b.name}</option>`).join('');
      }
      if (categorySelect) {
        categorySelect.innerHTML = '<option value="">(تلقائي - اختيار عام)</option>' +
          categories.map(c => `<option value="${c.id}">${c.name}</option>`).join('');
      }
    } catch (err) {
      console.warn('Brands/Categories fallback', err);
    }
  }

  // ══════════════════════════════════
  //  PRODUCTS MANAGEMENT
  // ══════════════════════════════════
  async function loadProducts() {
    try {
      productsBody.innerHTML = `<tr><td colspan="6" style="text-align: center; padding: 2rem; color: var(--text-muted);">جاري تحميل بضاعة اليوم... ⏳</td></tr>`;
      const res = await fetch(`${API_BASE}/Products?pageSize=100`, {
        headers: authHeaders()
      });
      if (!res.ok) throw new Error('فشل تحميل قائمة بضاعة اليوم');
      const result = await res.json();
      const products = result.data || result || [];

      if (statsProducts) statsProducts.textContent = result.count ?? products.length;
      renderProducts(products);
    } catch (err) {
      productsBody.innerHTML = `<tr><td colspan="6" style="text-align: center; color: var(--danger); padding: 2rem;">⚠️ ${err.message}</td></tr>`;
    }
  }

  function renderProducts(products) {
    if (!products.length) {
      productsBody.innerHTML = `
        <tr>
          <td colspan="6" class="empty-state">
            <div style="font-size: 2.5rem; margin-bottom: 0.5rem;">📦</div>
            <p>لا توجد بضاعة مسجلة لليوم بعد. ارفع أول صورة لبضاعتك الآن من النموذج بالأعلى!</p>
          </td>
        </tr>`;
      return;
    }

    productsBody.innerHTML = products.map(p => {
      const imgUrl = p.pictureUrl 
        ? (p.pictureUrl.startsWith('http') ? p.pictureUrl : window.location.origin + p.pictureUrl) 
        : '';
      const priceText = (p.price && Number(p.price) > 0)
        ? `<strong>${Number(p.price).toFixed(2)}</strong> ج.م`
        : `<span class="badge-price-request">عند الطلب 💬</span>`;

      return `
        <tr>
          <td>
            <img class="product-thumb" 
                 src="${imgUrl}" 
                 alt="${p.name}"
                 title="اضغط للتكبير"
                 onclick="window.showLightbox('${imgUrl}', '${encodeURIComponent(p.name)}')"
                 onerror="this.src='data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 width=%2260%22 height=%2260%22><rect fill=%22%23232340%22 width=%2260%22 height=%2260%22 rx=%228%22/><text x=%2230%22 y=%2235%22 text-anchor=%22middle%22 fill=%22%236c5ce7%22 font-size=%2222%22>📦</text></svg>'" />
          </td>
          <td>
            <div class="product-name-title">${p.name}</div>
            ${p.description && p.description !== p.name ? `<div class="product-desc-snippet">${p.description}</div>` : ''}
          </td>
          <td><div class="product-price-cell">${priceText}</div></td>
          <td><span class="badge-pill">${p.productCategory || 'عام'}</span></td>
          <td><span class="badge-pill">${p.productBrand || 'عام'}</span></td>
          <td>
            <div class="actions-cell">
              <button class="btn-action-icon btn-delete" onclick="window.deleteProduct(${p.id}, '${encodeURIComponent(p.name)}')" title="حذف هذه البضاعة من المتجر">
                🗑️ حذف
              </button>
            </div>
          </td>
        </tr>
      `;
    }).join('');
  }

  // ── Create Product (Price is OPTIONAL!) ──
  productForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!isTokenValid()) {
      showToast('انتهت جلسة تسجيل الدخول. يرجى تسجيل الدخول مجدداً.', 'error');
      clearToken();
      showLogin();
      return;
    }

    const btn = submitProductBtn;
    btn.disabled = true;
    btn.innerHTML = '<span>جاري رفع البضاعة ونشرها...</span> ⏳';

    const formData = new FormData();
    formData.append('Name', document.getElementById('name').value.trim());

    const description = document.getElementById('description').value.trim();
    if (description) formData.append('Description', description);

    // Price is OPTIONAL! If entered, send it; otherwise backend sets 0 (Upon Request)
    const priceVal = document.getElementById('price').value.trim();
    if (priceVal !== '') {
      formData.append('Price', priceVal);
    }

    if (categorySelect.value) formData.append('CategoryId', categorySelect.value);
    if (brandSelect.value) formData.append('BrandId', brandSelect.value);

    const imageFile = imageInput.files[0];
    if (imageFile) {
      formData.append('Image', imageFile);
    }

    try {
      const res = await fetch(`${API_BASE}/Products`, {
        method: 'POST',
        headers: authHeaders(),
        body: formData
      });

      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || `حدث خطأ أثناء الرفع (${res.status})`);
      }

      showToast('🎉 تم رفع البضاعة ونشرها في المتجر للزبائن بنجاح!', 'success');
      productForm.reset();
      imageInput.value = '';
      imagePreviewWrap.style.display = 'none';
      dropZonePrompt.style.display = 'block';
      await loadProducts();
    } catch (err) {
      showToast(err.message, 'error');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<span>نشر البضاعة في المتجر فوراً</span> 🚀';
    }
  });

  if (refreshProductsBtn) {
    refreshProductsBtn.addEventListener('click', loadProducts);
  }

  // ── Delete Product ──
  window.deleteProduct = async function(id, encodedName) {
    const name = decodeURIComponent(encodedName || 'هذا المنتج');
    if (!confirm(`هل أنت متأكد من حذف "${name}" من المتجر؟ لن يتمكن الزبائن من رؤيته بعد الحذف.`)) return;

    if (!isTokenValid()) {
      showToast('انتهت الجلسة، سجل دخول مرة أخرى', 'error');
      clearToken();
      showLogin();
      return;
    }

    try {
      const res = await fetch(`${API_BASE}/Products/${id}`, {
        method: 'DELETE',
        headers: authHeaders()
      });

      if (!res.ok) throw new Error(`فشل حذف المنتج (${res.status})`);

      showToast(`🗑️ تم حذف "${name}" بنجاح`, 'success');
      await loadProducts();
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  // ══════════════════════════════════
  //  ORDERS MANAGEMENT & LIFECYCLE
  // ══════════════════════════════════
  async function loadOrders() {
    if (!isTokenValid()) return;
    try {
      ordersBody.innerHTML = `<tr><td colspan="9" style="text-align: center; padding: 2rem; color: var(--text-muted);">جاري تحميل طلبات الزبائن وتحديث الحالات... ⏳</td></tr>`;
      const res = await fetch(`${API_BASE}/orders/all`, {
        headers: authHeaders()
      });
      if (!res.ok) throw new Error('فشل جلب الطلبات من السيرفر');
      allOrders = await res.json();

      updateOrderCounters();
      applyOrderFilters();
    } catch (err) {
      ordersBody.innerHTML = `<tr><td colspan="9" style="text-align: center; color: var(--danger); padding: 2rem;">⚠️ ${err.message}</td></tr>`;
    }
  }

  function updateOrderCounters() {
    const total = allOrders.length;
    const pending = allOrders.filter(o => isPending(o.status)).length;
    const accepted = allOrders.filter(o => isAccepted(o.status)).length;
    const shipped = allOrders.filter(o => isShipped(o.status)).length;
    const delivered = allOrders.filter(o => isDelivered(o.status)).length;
    const cancelled = allOrders.filter(o => isCancelled(o.status)).length;

    if (countAll) countAll.textContent = total;
    if (countPending) countPending.textContent = pending;
    if (countAccepted) countAccepted.textContent = accepted;
    if (countShipped) countShipped.textContent = shipped;
    if (countDelivered) countDelivered.textContent = delivered;
    if (countCancelled) countCancelled.textContent = cancelled;

    // Header & stats card updates
    if (statsOrders) statsOrders.textContent = total;
    if (statsPending) statsPending.textContent = pending;
    if (statsShipped) statsShipped.textContent = shipped;
    if (pendingOrdersBadge) {
      pendingOrdersBadge.textContent = pending;
      pendingOrdersBadge.style.display = pending > 0 ? 'inline-block' : 'none';
    }

    const totalSales = allOrders.reduce((sum, o) => sum + (o.total || 0), 0);
    if (statsSales) statsSales.textContent = totalSales.toFixed(2);
  }

  function isPending(status) {
    const s = String(status || '').toLowerCase();
    return s.includes('pending') || s.includes('انتظار');
  }
  function isAccepted(status) {
    const s = String(status || '').toLowerCase();
    return s.includes('accept') || s.includes('قبول') || s.includes('received');
  }
  function isShipped(status) {
    const s = String(status || '').toLowerCase();
    return s.includes('shipped') || s.includes('شحن');
  }
  function isDelivered(status) {
    const s = String(status || '').toLowerCase();
    return s.includes('delivered') || s.includes('تسليم');
  }
  function isCancelled(status) {
    const s = String(status || '').toLowerCase();
    return s.includes('cancel') || s.includes('failed') || s.includes('إلغاء') || s.includes('فشل');
  }

  function getStatusBadge(status) {
    if (isPending(status)) return `<span class="badge-status status-pending">⏳ بانتظار الموافقة</span>`;
    if (isAccepted(status)) return `<span class="badge-status status-accepted">✅ تم القبول والتجهيز</span>`;
    if (isShipped(status)) return `<span class="badge-status status-shipped">🚚 قيد الشحن والتوصيل</span>`;
    if (isDelivered(status)) return `<span class="badge-status status-delivered">📦 تم التسليم بنجاح</span>`;
    if (isCancelled(status)) return `<span class="badge-status status-failed">❌ تم الإلغاء</span>`;
    return `<span class="badge-status">${status}</span>`;
  }

  function applyOrderFilters() {
    let filtered = [...allOrders];

    // Filter by tab chip
    if (currentOrderFilter === 'pending') filtered = filtered.filter(o => isPending(o.status));
    else if (currentOrderFilter === 'accepted') filtered = filtered.filter(o => isAccepted(o.status));
    else if (currentOrderFilter === 'shipped') filtered = filtered.filter(o => isShipped(o.status));
    else if (currentOrderFilter === 'delivered') filtered = filtered.filter(o => isDelivered(o.status));
    else if (currentOrderFilter === 'cancelled') filtered = filtered.filter(o => isCancelled(o.status));

    // Filter by search query
    if (orderSearchQuery) {
      const q = orderSearchQuery.toLowerCase();
      filtered = filtered.filter(o => {
        const idMatch = String(o.id).includes(q);
        const emailMatch = (o.buyerEmail || '').toLowerCase().includes(q);
        const nameMatch = o.shippingAddress 
          ? `${o.shippingAddress.firstName || ''} ${o.shippingAddress.lastName || ''}`.toLowerCase().includes(q)
          : false;
        const streetMatch = o.shippingAddress && (o.shippingAddress.street || '').toLowerCase().includes(q);
        return idMatch || emailMatch || nameMatch || streetMatch;
      });
    }

    renderOrders(filtered);
  }

  function extractPhone(order) {
    if (order.shippingAddress && order.shippingAddress.street) {
      const match = order.shippingAddress.street.match(/01[0-9]{9}/);
      if (match) return match[0];
    }
    if (order.buyerEmail) {
      const match = order.buyerEmail.match(/01[0-9]{9}/);
      if (match) return match[0];
    }
    return '';
  }

  function renderOrders(orders) {
    if (!orders.length) {
      ordersBody.innerHTML = `
        <tr>
          <td colspan="9" class="empty-state">
            <div style="font-size: 2.5rem; margin-bottom: 0.5rem;">📋</div>
            <p>لا توجد طلبات مطابقة لهذا الفلتر حالياً.</p>
          </td>
        </tr>`;
      return;
    }

    ordersBody.innerHTML = orders.map(order => {
      const dateStr = order.date ? new Date(order.date).toLocaleString('ar-EG', {
        month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
      }) : 'اليوم';

      const addr = order.shippingAddress || {};
      const customerName = `${addr.firstName || ''} ${addr.lastName || ''}`.trim() || 'زبون المتجر';
      const phone = extractPhone(order);
      const cleanStreet = (addr.street || '').replace(/\(📱.*?\)/, '').trim();

      const itemsCount = (order.items && order.items.length) ? order.items.length : 0;
      const itemsListPreview = order.items && order.items.length
        ? order.items.map(i => `${i.productName} (×${i.quantity})`).join(', ')
        : '—';

      // WhatsApp Quick Link
      const waPhone = phone.startsWith('0') ? '20' + phone.substring(1) : phone;
      const waMsg = encodeURIComponent(`أهلاً بك يا ${customerName}! معك صاحب متجر SnapShop بخصوص طلبك رقم #${order.id}.`);
      const waLink = phone ? `https://wa.me/${waPhone}?text=${waMsg}` : null;

      // Status buttons depending on current lifecycle
      let actionButtons = '';
      if (isPending(order.status)) {
        actionButtons = `
          <button class="btn-step btn-accept" onclick="window.updateOrderStatus(${order.id}, 'Accepted')" title="قبول الطلب والبدء في تجهيزه">
            ✅ قبول الطلب
          </button>
          <button class="btn-step btn-cancel" onclick="window.updateOrderStatus(${order.id}, 'Cancelled')" title="إلغاء الطلب">
            ❌ إلغاء
          </button>
        `;
      } else if (isAccepted(order.status)) {
        actionButtons = `
          <button class="btn-step btn-ship" onclick="window.updateOrderStatus(${order.id}, 'Shipped')" title="تسليم الشحنة لمندوب التوصيل">
            🚚 بدء الشحن
          </button>
          <button class="btn-step btn-cancel" onclick="window.updateOrderStatus(${order.id}, 'Cancelled')" title="إلغاء الطلب">
            ❌ إلغاء
          </button>
        `;
      } else if (isShipped(order.status)) {
        actionButtons = `
          <button class="btn-step btn-deliver" onclick="window.updateOrderStatus(${order.id}, 'Delivered')" title="تم تسليم الشحنة للزبون وسداد قيمتها">
            📦 تم التسليم
          </button>
        `;
      } else if (isDelivered(order.status)) {
        actionButtons = `<span style="color: var(--success); font-weight: 700; font-size: 0.85rem;">🎉 تم اكتمال الطلب</span>`;
      } else if (isCancelled(order.status)) {
        actionButtons = `<button class="btn-step btn-accept" onclick="window.updateOrderStatus(${order.id}, 'Accepted')" title="إعادة تفعيل الطلب">🔄 إعادة تفعيل</button>`;
      }

      return `
        <tr id="order-row-${order.id}">
          <td>
            <div class="order-id-pill" onclick="window.viewOrderInvoice(${order.id})" title="عرض الفاتورة بالتفصيل">
              #${order.id}
            </div>
          </td>
          <td><span class="order-date-text">${dateStr}</span></td>
          <td>
            <div class="customer-info-box">
              <strong class="customer-name">${customerName}</strong>
              ${phone ? `
                <div class="customer-phone-row">
                  <a href="tel:${phone}" class="phone-link" title="اتصال هاتفي">📞 ${phone}</a>
                  ${waLink ? `<a href="${waLink}" target="_blank" class="wa-btn" title="محادثة واتساب">💬 واتساب</a>` : ''}
                </div>
              ` : `<div style="font-size: 0.8rem; color: var(--text-muted);">${order.buyerEmail || '—'}</div>`}
            </div>
          </td>
          <td>
            <div class="address-text-box">
              <div><strong>${addr.city || 'المنطقة'}</strong> - ${cleanStreet}</div>
              <small style="color: var(--text-muted);">${addr.country || 'مصر'}</small>
            </div>
          </td>
          <td>
            <div class="order-items-snippet" title="${itemsListPreview}">
              <strong>${itemsCount} صنف:</strong>
              <span>${itemsListPreview}</span>
            </div>
          </td>
          <td><span class="badge-delivery">${order.deliveryMethod || 'توصيل عادي'}</span></td>
          <td><strong class="order-total-price">${Number(order.total || 0).toFixed(2)} ج.م</strong></td>
          <td>${getStatusBadge(order.status)}</td>
          <td>
            <div class="order-actions-container">
              <div class="step-buttons-row">
                ${actionButtons}
              </div>
              <div style="display: flex; gap: 0.4rem; align-items: center; margin-top: 0.35rem;">
                <button class="btn-details-icon" onclick="window.viewOrderInvoice(${order.id})" title="عرض تفاصيل الفاتورة">
                  📄 الفاتورة
                </button>
                <select class="status-select-dropdown" onchange="window.updateOrderStatus(${order.id}, this.value)" title="تغيير الحالة يدوياً">
                  <option value="">تغيير الحالة...</option>
                  <option value="Pending">⏳ بانتظار الموافقة</option>
                  <option value="Accepted">✅ تم القبول</option>
                  <option value="Shipped">🚚 قيد الشحن</option>
                  <option value="Delivered">📦 تم التسليم</option>
                  <option value="Cancelled">❌ ملغي</option>
                </select>
              </div>
            </div>
          </td>
        </tr>
      `;
    }).join('');
  }

  // ── Update Order Status (All Use Cases) ──
  window.updateOrderStatus = async function(orderId, newStatus) {
    if (!newStatus) return;

    const arabicStatusMap = {
      'Pending': '⏳ بانتظار الموافقة',
      'Accepted': '✅ تم قبول الطلب وجاري تجهيزه',
      'Shipped': '🚚 تم شحن الطلب وخروجه مع المندوب',
      'Delivered': '📦 تم تسليم الطلب للزبون بنجاح',
      'Cancelled': '❌ تم إلغاء الطلب'
    };

    try {
      const res = await fetch(`${API_BASE}/orders/${orderId}/status?status=${newStatus}`, {
        method: 'PUT',
        headers: authHeaders()
      });

      if (!res.ok) throw new Error('فشل تحديث حالة الطلب على السيرفر');

      const updated = await res.json();
      showToast(`تم تحديث الطلب #${orderId} إلى: ${arabicStatusMap[newStatus] || newStatus}`, 'success');

      // Update in local state & re-render
      const idx = allOrders.findIndex(o => o.id === orderId);
      if (idx !== -1) {
        allOrders[idx].status = updated.status;
      }
      updateOrderCounters();
      applyOrderFilters();
    } catch (err) {
      showToast(err.message, 'error');
    }
  };

  // ── Order Filter Chips ──
  if (orderFilterChips) {
    orderFilterChips.querySelectorAll('.filter-chip').forEach(btn => {
      btn.addEventListener('click', () => {
        orderFilterChips.querySelectorAll('.filter-chip').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        currentOrderFilter = btn.dataset.filter;
        applyOrderFilters();
      });
    });
  }

  // ── Search Orders ──
  let searchTimeout = null;
  if (orderSearchInput) {
    orderSearchInput.addEventListener('input', (e) => {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => {
        orderSearchQuery = e.target.value.trim();
        applyOrderFilters();
      }, 300);
    });
  }

  if (refreshOrdersBtn) {
    refreshOrdersBtn.addEventListener('click', loadOrders);
  }

  // ══════════════════════════════════
  //  ORDER INVOICE / DETAILS MODAL
  // ══════════════════════════════════
  window.viewOrderInvoice = function(orderId) {
    const order = allOrders.find(o => o.id === orderId);
    if (!order) return;

    modalOrderTitle.textContent = `📋 تفاصيل وبوليصة الطلب #${order.id}`;

    const dateStr = order.date ? new Date(order.date).toLocaleString('ar-EG') : 'اليوم';
    const addr = order.shippingAddress || {};
    const phone = extractPhone(order);
    const cleanStreet = (addr.street || '').replace(/\(📱.*?\)/, '').trim();

    const itemsHtml = (order.items && order.items.length)
      ? order.items.map(i => `
          <div class="invoice-item-row">
            <div>
              <strong>${i.productName}</strong>
              <div style="font-size: 0.8rem; color: var(--text-muted);">الكمية: ${i.quantity} × ${Number(i.price) > 0 ? Number(i.price).toFixed(2) + ' ج.م' : 'عند الطلب'}</div>
            </div>
            <div style="font-weight: 700;">
              ${Number(i.price) > 0 ? (i.price * i.quantity).toFixed(2) + ' ج.م' : 'عند الطلب'}
            </div>
          </div>
        `).join('')
      : '<p>لا توجد تفاصيل للأصناف</p>';

    modalOrderBody.innerHTML = `
      <div class="invoice-container">
        <div class="invoice-meta-grid">
          <div>
            <label>رقم الطلب:</label>
            <h4>#${order.id}</h4>
          </div>
          <div>
            <label>تاريخ التسجيل:</label>
            <div>${dateStr}</div>
          </div>
          <div>
            <label>حالة الطلب:</label>
            <div>${getStatusBadge(order.status)}</div>
          </div>
          <div>
            <label>طريقة الشحن والتوصيل:</label>
            <div><strong>${order.deliveryMethod || 'توصيل عادي'}</strong> (${Number(order.deliveryMethodCost || 0).toFixed(2)} ج.م)</div>
          </div>
        </div>

        <div class="invoice-section">
          <h4>👤 بيانات المشتري والعنوان</h4>
          <p><strong>الاسم:</strong> ${addr.firstName || ''} ${addr.lastName || ''}</p>
          <p><strong>رقم الهاتف للتوصيل:</strong> ${phone ? `<a href="tel:${phone}" style="color: var(--primary); font-weight: 700;">${phone}</a>` : 'غير متوفر'}</p>
          <p><strong>العنوان التفصيلي:</strong> ${cleanStreet} - ${addr.city || ''} (${addr.country || 'مصر'})</p>
        </div>

        <div class="invoice-section">
          <h4>📦 الأصناف المطلوبة</h4>
          <div class="invoice-items-list">
            ${itemsHtml}
          </div>
        </div>

        <div class="invoice-totals">
          <div class="invoice-total-line">
            <span>المجموع الفرعي:</span>
            <span>${Number(order.subTotal || 0).toFixed(2)} ج.م</span>
          </div>
          <div class="invoice-total-line">
            <span>تكلفة التوصيل:</span>
            <span>${Number(order.deliveryMethodCost || 0).toFixed(2)} ج.م</span>
          </div>
          <div class="invoice-total-line grand-total">
            <span>الإجمالي النهائي المطلوب تحصيله:</span>
            <span>${Number(order.total || 0).toFixed(2)} ج.م</span>
          </div>
        </div>
      </div>
    `;

    orderDetailModal.classList.add('open');
  };

  function closeDetailModal() {
    orderDetailModal.classList.remove('open');
  }
  if (closeDetailModalBtn) closeDetailModalBtn.addEventListener('click', closeDetailModal);
  if (closeDetailModalBtn2) closeDetailModalBtn2.addEventListener('click', closeDetailModal);

  // ══════════════════════════════════
  //  LIGHTBOX FOR PRODUCT IMAGES
  // ══════════════════════════════════
  window.showLightbox = function(src, encodedAlt) {
    if (!src) return;
    lightboxImg.src = src;
    lightboxImg.alt = decodeURIComponent(encodedAlt || 'صورة البضاعة');
    lightboxModal.classList.add('open');
  };

  if (lightboxClose) {
    lightboxClose.addEventListener('click', () => lightboxModal.classList.remove('open'));
  }
  if (lightboxModal) {
    lightboxModal.addEventListener('click', (e) => {
      if (e.target === lightboxModal) lightboxModal.classList.remove('open');
    });
  }

  // ══════════════════════════════════
  //  INITIALIZATION
  // ══════════════════════════════════
  async function loadDashboardData() {
    await Promise.all([
      loadProducts(),
      loadOrders(),
      loadBrandsAndCategories()
    ]);
  }

  if (isTokenValid()) {
    showDashboard();
    loadDashboardData();
  } else {
    showLogin();
  }

})();
