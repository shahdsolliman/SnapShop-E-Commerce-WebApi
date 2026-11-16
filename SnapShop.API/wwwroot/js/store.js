/* ══════════════════════════════════════════════════════
   SnapShop Storefront — Customer Experience Logic
   Handles:
   - Today's Goods browsing with real photos & optional prices ("عند الطلب")
   - Cart & Instant Checkout with Phone and Address
   - Seamless Order Creation & Storage
   - Live Order Tracking & Lifecycle Timeline (Pending -> Accepted -> Shipped -> Delivered)
   - Order Lookup by ID & WhatsApp shop owner connection
   ══════════════════════════════════════════════════════ */

(() => {
  'use strict';

  const API_BASE = window.location.origin + '/api';

  // ── State ──
  let basketId = localStorage.getItem('snapshop_basket_id');
  if (!basketId) {
    basketId = 'basket_' + Math.random().toString(36).substring(2, 12);
    localStorage.setItem('snapshop_basket_id', basketId);
  }

  let cart = { id: basketId, items: [] };
  let deliveryMethods = [];
  let selectedDeliveryMethod = null;
  let allProducts = [];
  let currentFilter = { brandId: '', categoryId: '', sort: '', search: '' };
  let currentTrackingOrderId = null;

  // ── Local Storage My Orders ──
  function getMyOrders() {
    try {
      return JSON.parse(localStorage.getItem('snapshop_my_orders')) || [];
    } catch {
      return [];
    }
  }

  function saveOrderToMyOrders(order) {
    const list = getMyOrders();
    const existingIdx = list.findIndex(o => o.id === order.id);
    if (existingIdx !== -1) {
      list[existingIdx] = order;
    } else {
      list.unshift(order);
    }
    localStorage.setItem('snapshop_my_orders', JSON.stringify(list.slice(0, 20)));
    updateMyOrdersBadge();
  }

  function updateMyOrdersBadge() {
    const badge = document.getElementById('myOrdersBadge');
    const orders = getMyOrders();
    if (badge) {
      badge.textContent = orders.length;
      badge.style.display = orders.length > 0 ? 'inline-block' : 'none';
    }
  }

  // ── DOM Elements ──
  const productsGrid         = document.getElementById('productsGrid');
  const searchInput          = document.getElementById('searchInput');
  const sortSelect           = document.getElementById('sortSelect');
  const brandPills           = document.getElementById('brandPills');
  const categoryPills        = document.getElementById('categoryPills');
  const cartBtn              = document.getElementById('cartBtn');
  const cartBadge            = document.getElementById('cartBadge');
  const cartDrawer           = document.getElementById('cartDrawer');
  const cartOverlay          = document.getElementById('cartOverlay');
  const closeCartBtn         = document.getElementById('closeCartBtn');
  const cartItemsContainer   = document.getElementById('cartItems');
  const cartSubtotalEl       = document.getElementById('cartSubtotal');
  const cartShippingEl       = document.getElementById('cartShipping');
  const cartTotalEl          = document.getElementById('cartTotal');
  const uponRequestNote      = document.getElementById('uponRequestNote');
  const checkoutBtn          = document.getElementById('checkoutBtn');
  const checkoutModal        = document.getElementById('checkoutModal');
  const closeCheckoutBtn     = document.getElementById('closeCheckoutBtn');
  const checkoutForm         = document.getElementById('checkoutForm');
  const submitOrderBtn       = document.getElementById('submitOrderBtn');
  const deliveryList         = document.getElementById('deliveryList');
  const toastContainer       = document.getElementById('toastContainer');

  // Tracking Modal Elements
  const trackingModal        = document.getElementById('trackingModal');
  const closeTrackingBtn     = document.getElementById('closeTrackingBtn');
  const trackerIcon          = document.getElementById('trackerIcon');
  const trackerTitle         = document.getElementById('trackerTitle');
  const trackerSubtitle      = document.getElementById('trackerSubtitle');
  const trackingOrderId      = document.getElementById('trackingOrderId');
  const trackerSummaryContent= document.getElementById('trackerSummaryContent');
  const refreshStatusBtn     = document.getElementById('refreshStatusBtn');
  const trackWhatsAppBtn     = document.getElementById('trackWhatsAppBtn');
  const cancelledBanner      = document.getElementById('cancelledBanner');

  // Step elements
  const stepReceived         = document.getElementById('stepReceived');
  const stepAccepted         = document.getElementById('stepAccepted');
  const stepShipped          = document.getElementById('stepShipped');
  const stepDelivered        = document.getElementById('stepDelivered');
  const stepTimeReceived     = document.getElementById('stepTimeReceived');
  const stepTimeAccepted     = document.getElementById('stepTimeAccepted');
  const stepTimeShipped      = document.getElementById('stepTimeShipped');
  const stepTimeDelivered    = document.getElementById('stepTimeDelivered');

  // My Orders Modal
  const trackNavBtn          = document.getElementById('trackNavBtn');
  const myOrdersModal        = document.getElementById('myOrdersModal');
  const closeMyOrdersBtn     = document.getElementById('closeMyOrdersBtn');
  const lookupOrderIdInput   = document.getElementById('lookupOrderIdInput');
  const btnLookupOrder       = document.getElementById('btnLookupOrder');
  const recentOrdersList     = document.getElementById('recentOrdersList');

  // Lightbox
  const lightboxModal        = document.getElementById('lightboxModal');
  const lightboxImg          = document.getElementById('lightboxImg');
  const lightboxClose        = document.getElementById('lightboxClose');

  // ── Toasts ──
  function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    const icon = type === 'success' ? '✅' : '⚠️';
    toast.innerHTML = `<span>${icon}</span> <span>${message}</span>`;
    toastContainer.appendChild(toast);
    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(-10px)';
      setTimeout(() => toast.remove(), 400);
    }, 3800);
  }

  function formatPrice(val) {
    if (val === null || val === undefined || Number(val) === 0) {
      return '<span class="price-upon-request">السعر عند الطلب 💬</span>';
    }
    return `<strong>${Number(val).toFixed(2)}</strong> ج.م`;
  }

  function getFallbackImg(name) {
    return `data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="300" height="240" viewBox="0 0 300 240"><rect fill="%22%2316172b%22" width="100%" height="100%"/><text x="50%" y="45%" text-anchor="middle" fill="%22%23ff6b6b%22" font-size="40">🛍️</text><text x="50%" y="65%" text-anchor="middle" fill="%22%239e9ea7%22" font-family="sans-serif" font-size="14">${encodeURIComponent(name || 'بضاعة اليوم')}</text></svg>`;
  }

  // ══════════════════════════════════════
  //  FETCH PRODUCTS & FILTERS
  // ══════════════════════════════════════
  async function loadInitialData() {
    updateMyOrdersBadge();
    await Promise.all([
      loadBrands(),
      loadCategories(),
      loadDeliveryMethods(),
      loadBasket(),
      fetchProducts()
    ]);
  }

  async function loadBrands() {
    try {
      const res = await fetch(`${API_BASE}/products/brands`);
      if (!res.ok) return;
      const brands = await res.json();
      brandPills.innerHTML = `<button class="pill-btn active" data-id="">الكل</button>` +
        brands.map(b => `<button class="pill-btn" data-id="${b.id}">${b.name}</button>`).join('');

      brandPills.querySelectorAll('.pill-btn').forEach(btn => {
        btn.addEventListener('click', () => {
          brandPills.querySelectorAll('.pill-btn').forEach(b => b.classList.remove('active'));
          btn.classList.add('active');
          currentFilter.brandId = btn.dataset.id;
          fetchProducts();
        });
      });
    } catch (e) { console.warn('Failed to load brands', e); }
  }

  async function loadCategories() {
    try {
      const res = await fetch(`${API_BASE}/products/categories`);
      if (!res.ok) return;
      const categories = await res.json();
      categoryPills.innerHTML = `<button class="pill-btn active" data-id="">الكل</button>` +
        categories.map(c => `<button class="pill-btn" data-id="${c.id}">${c.name}</button>`).join('');

      categoryPills.querySelectorAll('.pill-btn').forEach(btn => {
        btn.addEventListener('click', () => {
          categoryPills.querySelectorAll('.pill-btn').forEach(b => b.classList.remove('active'));
          btn.classList.add('active');
          currentFilter.categoryId = btn.dataset.id;
          fetchProducts();
        });
      });
    } catch (e) { console.warn('Failed to load categories', e); }
  }

  async function loadDeliveryMethods() {
    try {
      const res = await fetch(`${API_BASE}/orders/deliveryMethods`);
      if (!res.ok) return;
      deliveryMethods = await res.json();
      if (deliveryMethods.length > 0) {
        selectedDeliveryMethod = deliveryMethods[0];
      }
      renderDeliveryOptions();
    } catch (e) { console.warn('Failed to load delivery methods', e); }
  }

  async function fetchProducts() {
    try {
      productsGrid.innerHTML = `
        <div style="grid-column: 1/-1; text-align: center; padding: 4rem 1rem; color: var(--text-muted);">
          <div style="font-size: 2.5rem; margin-bottom: 0.5rem; animation: spin 1s linear infinite;">⏳</div>
          <p>جاري تحميل صور بضاعة اليوم المعروضة...</p>
        </div>`;

      const params = new URLSearchParams({
        pageIndex: 1,
        pageSize: 60
      });
      if (currentFilter.brandId) params.append('brandId', currentFilter.brandId);
      if (currentFilter.categoryId) params.append('categoryId', currentFilter.categoryId);
      if (currentFilter.sort) params.append('sort', currentFilter.sort);
      if (currentFilter.search) params.append('search', currentFilter.search);

      const res = await fetch(`${API_BASE}/products?${params.toString()}`);
      if (!res.ok) throw new Error('فشل جلب المنتجات');
      const data = await res.json();
      allProducts = data.data || [];
      renderProducts(allProducts);
    } catch (err) {
      productsGrid.innerHTML = `
        <div style="grid-column: 1/-1; text-align: center; padding: 3rem; color: var(--danger);">
          <p>⚠️ ${err.message || 'تعذر تحميل المنتجات حالياً. تأكد من تشغيل السيرفر.'}</p>
        </div>`;
    }
  }

  function renderProducts(products) {
    if (!products.length) {
      productsGrid.innerHTML = `
        <div style="grid-column: 1/-1; text-align: center; padding: 4rem 1rem; color: var(--text-muted);">
          <div style="font-size: 3.5rem; margin-bottom: 1rem; opacity: 0.5;">📦</div>
          <h3>لا توجد بضاعة مطابقة للبحث حالياً</h3>
          <p>يرجى تجربة فلتر آخر أو تصفح باقي الأقسام.</p>
        </div>`;
      return;
    }

    productsGrid.innerHTML = products.map(p => {
      const imgUrl = p.pictureUrl 
        ? (p.pictureUrl.startsWith('http') ? p.pictureUrl : window.location.origin + p.pictureUrl) 
        : getFallbackImg(p.name);
      
      const hasPrice = p.price && Number(p.price) > 0;
      const priceDisplay = hasPrice
        ? `<div class="product-price"><strong>${Number(p.price).toFixed(2)}</strong> <small>ج.م</small></div>`
        : `<div class="product-price upon-request"><span class="badge-price-tag">💬 السعر عند الطلب</span></div>`;

      return `
        <article class="product-card glass" id="product-${p.id}">
          <div class="product-media" onclick="window.showLightbox('${imgUrl}', '${encodeURIComponent(p.name)}')">
            <div class="product-badges">
              ${p.productBrand ? `<span class="badge badge-brand">${p.productBrand}</span>` : ''}
              ${p.productCategory ? `<span class="badge badge-category">${p.productCategory}</span>` : ''}
            </div>
            <img src="${imgUrl}" alt="${p.name}" loading="lazy" onerror="this.src='${getFallbackImg(p.name)}'" />
            <span class="zoom-hint">🔍 انقر للتكبير</span>
          </div>
          <div class="product-content">
            <h3 class="product-name" title="${p.name}">${p.name}</h3>
            <p class="product-desc">${p.description || 'بضاعة ممتازة بجودة عالية متوفرة في المحل اليوم.'}</p>
            <div class="product-footer">
              ${priceDisplay}
              <div class="card-btn-row">
                <button class="btn-add-cart" onclick="window.addToCart(${p.id})">
                  <span>🛒</span> أضف للسلة
                </button>
                <button class="btn-buy-instant" onclick="window.instantBuy(${p.id})" title="شراء وطلب فوري">
                  ⚡ اطلب فوراً
                </button>
              </div>
            </div>
          </div>
        </article>
      `;
    }).join('');
  }

  // ══════════════════════════════════════
  //  BASKET & CART MANAGEMENT
  // ══════════════════════════════════════
  async function loadBasket() {
    try {
      const res = await fetch(`${API_BASE}/basket?id=${basketId}`);
      if (res.ok) {
        const data = await res.json();
        if (data && data.items) {
          cart = data;
          updateCartUI();
        }
      }
    } catch (e) {
      console.warn('Could not sync basket with server', e);
    }
  }

  async function saveBasket() {
    try {
      await fetch(`${API_BASE}/basket`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(cart)
      });
    } catch (e) {
      console.warn('Failed to save basket', e);
    }
  }

  window.addToCart = async function(productId) {
    const product = allProducts.find(p => p.id === productId);
    if (!product) return;

    const existing = cart.items.find(i => i.id === productId);
    if (existing) {
      existing.quantity += 1;
    } else {
      cart.items.push({
        id: product.id,
        name: product.name,
        description: product.description || product.name,
        price: Number(product.price) || 0,
        quantity: 1,
        pictureUrl: product.pictureUrl || '',
        brand: product.productBrand || 'عام',
        category: product.productCategory || 'عام'
      });
    }

    updateCartUI();
    await saveBasket();
    showToast(`تمت إضافة "${product.name}" إلى السلة 🛍️`);
    openCart();
  };

  window.instantBuy = async function(productId) {
    await window.addToCart(productId);
    closeCart();
    checkoutModal.classList.add('open');
  };

  window.updateQty = async function(productId, delta) {
    const item = cart.items.find(i => i.id === productId);
    if (!item) return;

    item.quantity += delta;
    if (item.quantity <= 0) {
      cart.items = cart.items.filter(i => i.id !== productId);
    }

    updateCartUI();
    await saveBasket();
  };

  window.removeFromCart = async function(productId) {
    cart.items = cart.items.filter(i => i.id !== productId);
    updateCartUI();
    await saveBasket();
    showToast('تم حذف المنتج من السلة');
  };

  function updateCartUI() {
    const totalCount = cart.items.reduce((sum, item) => sum + item.quantity, 0);
    cartBadge.textContent = totalCount;

    if (cart.items.length === 0) {
      cartItemsContainer.innerHTML = `
        <div class="cart-empty">
          <div class="cart-empty-icon">🛒</div>
          <h4>سلة التسوق فارغة</h4>
          <p>تصفح بضاعة اليوم واختر ما يعجبك ليصلك لباب بيتك!</p>
        </div>`;
      cartSubtotalEl.textContent = '0.00 ج.م';
      cartShippingEl.textContent = '0.00 ج.م';
      cartTotalEl.textContent = '0.00 ج.م';
      if (uponRequestNote) uponRequestNote.style.display = 'none';
      checkoutBtn.disabled = true;
      return;
    }

    checkoutBtn.disabled = false;
    let hasUponRequest = false;
    const subtotal = cart.items.reduce((sum, item) => {
      if (item.price === 0) hasUponRequest = true;
      return sum + (item.price * item.quantity);
    }, 0);

    const shipping = selectedDeliveryMethod ? Number(selectedDeliveryMethod.cost) : 0;
    const total = subtotal + shipping;

    cartSubtotalEl.textContent = subtotal > 0 ? `${subtotal.toFixed(2)} ج.م` : 'عند الطلب';
    cartShippingEl.textContent = shipping > 0 ? `${shipping.toFixed(2)} ج.م` : 'مجاناً';
    cartTotalEl.textContent = total > 0 ? `${total.toFixed(2)} ج.م` : 'عند الطلب';

    if (uponRequestNote) {
      uponRequestNote.style.display = hasUponRequest ? 'block' : 'none';
    }

    cartItemsContainer.innerHTML = cart.items.map(item => {
      const imgUrl = item.pictureUrl 
        ? (item.pictureUrl.startsWith('http') ? item.pictureUrl : window.location.origin + item.pictureUrl) 
        : getFallbackImg(item.name);
      
      const priceHtml = item.price > 0
        ? `<strong>${(item.price * item.quantity).toFixed(2)}</strong> ج.م`
        : `<span class="badge-price-tag">عند الطلب 💬</span>`;

      return `
        <div class="cart-item">
          <img class="cart-item-img" src="${imgUrl}" alt="${item.name}" onerror="this.src='${getFallbackImg(item.name)}'" />
          <div class="cart-item-info">
            <div class="cart-item-title">${item.name}</div>
            <div class="cart-item-price">${priceHtml}</div>
            <div class="cart-qty-controls">
              <button class="btn-qty" onclick="window.updateQty(${item.id}, -1)">-</button>
              <span class="cart-qty-value">${item.quantity}</span>
              <button class="btn-qty" onclick="window.updateQty(${item.id}, 1)">+</button>
            </div>
          </div>
          <button class="btn-item-remove" onclick="window.removeFromCart(${item.id})" title="حذف">🗑️</button>
        </div>
      `;
    }).join('');
  }

  function renderDeliveryOptions() {
    if (!deliveryList || !deliveryMethods.length) return;
    deliveryList.innerHTML = deliveryMethods.map(dm => `
      <label class="delivery-option ${selectedDeliveryMethod && selectedDeliveryMethod.id === dm.id ? 'selected' : ''}">
        <div>
          <input type="radio" name="deliveryMethod" value="${dm.id}" ${selectedDeliveryMethod && selectedDeliveryMethod.id === dm.id ? 'checked' : ''} />
          <strong>${dm.shortName}</strong> (${dm.deliveryTime})
        </div>
        <div><strong>${Number(dm.cost) === 0 ? 'مجاناً' : Number(dm.cost).toFixed(2) + ' ج.م'}</strong></div>
      </label>
    `).join('');

    deliveryList.querySelectorAll('input[name="deliveryMethod"]').forEach(input => {
      input.addEventListener('change', (e) => {
        selectedDeliveryMethod = deliveryMethods.find(m => m.id === Number(e.target.value));
        renderDeliveryOptions();
        updateCartUI();
      });
    });
  }

  // ══════════════════════════════════════
  //  DRAWER & MODAL CONTROLS
  // ══════════════════════════════════════
  function openCart() {
    cartDrawer.classList.add('open');
    cartOverlay.classList.add('open');
  }

  function closeCart() {
    cartDrawer.classList.remove('open');
    cartOverlay.classList.remove('open');
  }

  cartBtn.addEventListener('click', openCart);
  closeCartBtn.addEventListener('click', closeCart);
  cartOverlay.addEventListener('click', closeCart);

  checkoutBtn.addEventListener('click', () => {
    closeCart();
    renderDeliveryOptions();
    checkoutModal.classList.add('open');
  });

  closeCheckoutBtn.addEventListener('click', () => {
    checkoutModal.classList.remove('open');
  });

  // ══════════════════════════════════════
  //  CHECKOUT & ORDER CREATION
  // ══════════════════════════════════
  checkoutForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const btn = submitOrderBtn;
    btn.disabled = true;
    btn.innerHTML = '<span>جاري إرسال الطلب للمحل...</span> ⏳';

    const firstName = document.getElementById('firstName').value.trim();
    const lastName  = document.getElementById('lastName').value.trim();
    const phone     = document.getElementById('phone').value.trim();
    const street    = document.getElementById('street').value.trim();
    const city      = document.getElementById('city').value.trim();
    const country   = document.getElementById('country').value.trim();

    // Prepare payload
    const orderPayload = {
      basketId: basketId,
      deliveryMethodId: selectedDeliveryMethod ? selectedDeliveryMethod.id : 1,
      shippingAddress: {
        firstName: firstName,
        lastName: lastName,
        street: `${street} (📱 ${phone})`,
        city: city,
        country: country
      },
      buyerEmail: `customer_${phone}@snapshop.local`,
      phone: phone
    };

    try {
      const res = await fetch(`${API_BASE}/orders`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(orderPayload)
      });

      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.message || 'فشل تسجيل الطلب. يرجى مراجعة بياناتك والمحاولة مرة أخرى.');
      }

      const createdOrder = await res.json();

      // Clear local cart
      cart.items = [];
      updateCartUI();
      await fetch(`${API_BASE}/basket?id=${basketId}`, { method: 'DELETE' }).catch(() => {});

      // Reset basket id for future orders
      basketId = 'basket_' + Math.random().toString(36).substring(2, 12);
      localStorage.setItem('snapshop_basket_id', basketId);
      cart.id = basketId;

      checkoutModal.classList.remove('open');
      checkoutForm.reset();

      // Save order to My Orders localStorage
      saveOrderToMyOrders(createdOrder);

      // Open Live Order Tracking Modal immediately
      openOrderTracker(createdOrder.id, true);
      showToast('🎉 تم تأكيد طلبك بنجاح! يمكنك الآن متابعة مراحل تنفيذه.');

    } catch (err) {
      showToast(err.message, 'error');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<span>تأكيد الطلب وإرساله للمحل فوراً</span> 🚀';
    }
  });

  // ══════════════════════════════════════
  //  LIVE ORDER TRACKING (Pending -> Accepted -> Shipped -> Delivered)
  // ══════════════════════════════════════
  async function openOrderTracker(orderId, isNewOrder = false) {
    currentTrackingOrderId = orderId;
    trackingOrderId.textContent = `#${orderId}`;

    if (isNewOrder) {
      trackerIcon.textContent = '🎉';
      trackerTitle.textContent = 'تم استلام طلبك بنجاح!';
      trackerSubtitle.textContent = 'طلبك الآن مسجل لدى صاحب المحل، يمكنك متابعة مراحل تجهيزه وشحنه وتوصيله لحظة بلحظة أدناه.';
    } else {
      trackerIcon.textContent = '📦';
      trackerTitle.textContent = `متابعة الطلب #${orderId}`;
      trackerSubtitle.textContent = 'تتبع حي ومباشر لمراحل تجهيز وتسليم شحنتك من المحل.';
    }

    trackingModal.classList.add('open');
    await refreshOrderStatus(orderId);
  }

  async function refreshOrderStatus(orderId) {
    if (!orderId) return;

    refreshStatusBtn.disabled = true;
    refreshStatusBtn.innerHTML = 'جاري التحديث... ⏳';

    try {
      const res = await fetch(`${API_BASE}/orders/track/${orderId}`);
      if (!res.ok) throw new Error('تعذر العثور على تفاصيل الطلب');
      const order = await res.json();

      // Update in My Orders cache
      saveOrderToMyOrders(order);

      renderTimeline(order);
      renderTrackerSummary(order);
      setupWhatsAppLink(order);

    } catch (err) {
      showToast(err.message, 'error');
    } finally {
      refreshStatusBtn.disabled = false;
      refreshStatusBtn.innerHTML = '🔄 تحديث حالة الطلب الآن';
    }
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

  function renderTimeline(order) {
    // Reset classes
    [stepReceived, stepAccepted, stepShipped, stepDelivered].forEach(step => {
      step.classList.remove('active', 'completed');
    });

    const status = order.status;
    const dateFormatted = order.date ? new Date(order.date).toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' }) : 'اليوم';

    if (isCancelled(status)) {
      cancelledBanner.style.display = 'block';
      return;
    } else {
      cancelledBanner.style.display = 'none';
    }

    // Step 1: Received
    stepReceived.classList.add('completed');
    stepTimeReceived.textContent = dateFormatted;

    if (isPending(status)) {
      stepReceived.classList.add('active');
    } else if (isAccepted(status)) {
      stepAccepted.classList.add('active', 'completed');
      stepTimeAccepted.textContent = 'تم القبول وجاري التجهيز';
    } else if (isShipped(status)) {
      stepAccepted.classList.add('completed');
      stepShipped.classList.add('active', 'completed');
      stepTimeShipped.textContent = 'مع المندوب في الطريق';
    } else if (isDelivered(status)) {
      stepAccepted.classList.add('completed');
      stepShipped.classList.add('completed');
      stepDelivered.classList.add('active', 'completed');
      stepTimeDelivered.textContent = 'تم التسليم بنجاح';
    }
  }

  function renderTrackerSummary(order) {
    const addr = order.shippingAddress || {};
    const cleanStreet = (addr.street || '').replace(/\(📱.*?\)/, '').trim();

    const itemsHtml = (order.items && order.items.length)
      ? order.items.map(i => `
          <div class="tracker-item-row">
            <span>${i.productName} (×${i.quantity})</span>
            <strong>${Number(i.price) > 0 ? (i.price * i.quantity).toFixed(2) + ' ج.م' : 'عند الطلب'}</strong>
          </div>
        `).join('')
      : '';

    trackerSummaryContent.innerHTML = `
      <div class="tracker-details-list">
        <div class="tracker-detail-line">
          <span>العنوان:</span>
          <strong>${cleanStreet} - ${addr.city || ''}</strong>
        </div>
        <div class="tracker-detail-line">
          <span>طريقة الشحن:</span>
          <strong>${order.deliveryMethod || 'توصيل عادي'} (${Number(order.deliveryMethodCost || 0).toFixed(2)} ج.م)</strong>
        </div>
        <div class="tracker-items-wrap">
          ${itemsHtml}
        </div>
        <div class="tracker-detail-line tracker-total-line">
          <span>الإجمالي النهائي المطلوب:</span>
          <strong>${Number(order.total || 0).toFixed(2)} ج.م</strong>
        </div>
      </div>
    `;
  }

  function setupWhatsAppLink(order) {
    if (!trackWhatsAppBtn) return;
    const msg = encodeURIComponent(`مرحباً صاحب متجر SnapShop! أستفسر بخصوص طلبي رقم #${order.id} باسم ${order.shippingAddress?.firstName || ''}.`);
    // Provide general shop link
    trackWhatsAppBtn.href = `https://wa.me/?text=${msg}`;
  }

  if (refreshStatusBtn) {
    refreshStatusBtn.addEventListener('click', () => {
      if (currentTrackingOrderId) {
        refreshOrderStatus(currentTrackingOrderId);
        showToast('تم تحديث حالة الطلب من السيرفر ✅');
      }
    });
  }

  if (closeTrackingBtn) {
    closeTrackingBtn.addEventListener('click', () => {
      trackingModal.classList.remove('open');
    });
  }

  // ══════════════════════════════════════
  //  MY ORDERS LOOKUP MODAL
  // ══════════════════════════════════════
  if (trackNavBtn) {
    trackNavBtn.addEventListener('click', () => {
      renderRecentOrdersList();
      myOrdersModal.classList.add('open');
    });
  }

  if (closeMyOrdersBtn) {
    closeMyOrdersBtn.addEventListener('click', () => {
      myOrdersModal.classList.remove('open');
    });
  }

  if (btnLookupOrder) {
    btnLookupOrder.addEventListener('click', () => {
      const id = lookupOrderIdInput.value.trim();
      if (!id) {
        showToast('يرجى كتابة رقم الطلب أولاً', 'error');
        return;
      }
      myOrdersModal.classList.remove('open');
      openOrderTracker(Number(id));
    });
  }

  function renderRecentOrdersList() {
    const list = getMyOrders();
    if (!list.length) {
      recentOrdersList.innerHTML = `
        <div style="text-align: center; padding: 2rem; color: var(--text-muted);">
          <div style="font-size: 2rem; margin-bottom: 0.5rem;">📦</div>
          <p>لا توجد طلبات سابقة محفوظة في هذا المتصفح.</p>
          <small>يمكنك البحث برقم الطلب في الخانة بالأعلى.</small>
        </div>`;
      return;
    }

    recentOrdersList.innerHTML = list.map(o => {
      const dateStr = o.date ? new Date(o.date).toLocaleDateString('ar-EG', { month: 'short', day: 'numeric' }) : 'مؤخراً';
      let statusArabic = 'قيد الانتظار ⏳';
      if (isAccepted(o.status)) statusArabic = 'تم القبول ✅';
      else if (isShipped(o.status)) statusArabic = 'قيد الشحن 🚚';
      else if (isDelivered(o.status)) statusArabic = 'تم التسليم 📦';
      else if (isCancelled(o.status)) statusArabic = 'ملغي ❌';

      return `
        <div class="recent-order-card" onclick="window.trackFromList(${o.id})">
          <div>
            <strong>طلب رقم #${o.id}</strong>
            <div style="font-size: 0.8rem; color: var(--text-muted);">${dateStr} • الإجمالي: ${Number(o.total || 0).toFixed(2)} ج.م</div>
          </div>
          <div>
            <span class="badge-status">${statusArabic}</span>
          </div>
        </div>
      `;
    }).join('');
  }

  window.trackFromList = function(id) {
    myOrdersModal.classList.remove('open');
    openOrderTracker(id);
  };

  // ══════════════════════════════════════
  //  LIGHTBOX
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

  // ══════════════════════════════════════
  //  SEARCH & SORT
  // ══════════════════════════════════
  let searchTimeout = null;
  searchInput.addEventListener('input', (e) => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      currentFilter.search = e.target.value.trim();
      fetchProducts();
    }, 400);
  });

  sortSelect.addEventListener('change', (e) => {
    currentFilter.sort = e.target.value;
    fetchProducts();
  });

  // ── Initialize ──
  loadInitialData();

})();
