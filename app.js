const DB_KEY = 'teklif-program-v2';

const state = {
  route: { page: 'home', plumbingTypeId: null, workDetailId: null },
  db: loadDB(),
};

const app = document.getElementById('app');
const navButtons = [...document.querySelectorAll('[data-nav]')];

document.getElementById('resetDemo').addEventListener('click', () => {
  state.db = getDemoData();
  saveDB();
  goHome();
});

navButtons.forEach((btn) => btn.addEventListener('click', () => navigate(btn.dataset.nav)));

function loadDB() {
  const raw = localStorage.getItem(DB_KEY);
  if (!raw) return { plumbingTypes: [], workDetails: [], brands: [], materialDefs: [], materialRows: [] };
  try {
    const db = JSON.parse(raw);
    return {
      plumbingTypes: db.plumbingTypes || [],
      workDetails: db.workDetails || [],
      brands: db.brands || [],
      materialDefs: db.materialDefs || [],
      materialRows: db.materialRows || [],
    };
  } catch {
    return { plumbingTypes: [], workDetails: [], brands: [], materialDefs: [], materialRows: [] };
  }
}

function saveDB() {
  localStorage.setItem(DB_KEY, JSON.stringify(state.db));
}

function uid(prefix) { return `${prefix}_${Math.random().toString(36).slice(2, 10)}`; }
function money(v) { return `${(Number(v) || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺`; }

function calcRow(row) {
  const qty = Number(row.quantity) || 0;
  const list = Number(row.listPrice) || 0;
  const disc = Number(row.discount) || 0;
  const labor = Number(row.laborUnitPrice) || 0;
  const unit = list * (1 - disc / 100);
  const materialTotal = unit * qty;
  const laborTotal = labor * qty;
  return { unit, materialTotal, laborTotal, total: materialTotal + laborTotal };
}

function rowsOf(workDetailId) { return state.db.materialRows.filter((x) => x.workDetailId === workDetailId); }
function workDetailsOf(plumbingTypeId) { return state.db.workDetails.filter((x) => x.plumbingTypeId === plumbingTypeId); }
function workDetailTotal(id) { return rowsOf(id).reduce((s, r) => s + calcRow(r).total, 0); }
function plumbingTotal(id) { return workDetailsOf(id).reduce((s, wd) => s + workDetailTotal(wd.id), 0); }
function grandTotal() { return state.db.plumbingTypes.reduce((s, p) => s + plumbingTotal(p.id), 0); }

function navigate(page, payload = {}) {
  state.route = { page, plumbingTypeId: payload.plumbingTypeId || null, workDetailId: payload.workDetailId || null };
  navButtons.forEach((b) => b.classList.toggle('active', b.dataset.nav === page));
  render();
}
function goHome() { navigate('home'); }

function render() {
  if (state.route.page === 'home') return renderHome();
  if (state.route.page === 'settings') return renderSettings();
  if (state.route.page === 'work-details') return renderWorkDetails(state.route.plumbingTypeId);
  if (state.route.page === 'materials') return renderMaterials(state.route.workDetailId);
}

function renderHome() {
  const rows = state.db.plumbingTypes.map((p) => `<tr>
      <td>${p.name}</td>
      <td>${money(plumbingTotal(p.id))}</td>
      <td><button class="soft" data-open="${p.id}">İçeri Gir</button></td>
    </tr>`).join('');

  app.innerHTML = `<section class="card">
      <h2>Ana Sayfa</h2>
      <p>Sadece tesisat türü ve tutar görünür.</p>
      <table>
        <thead><tr><th>Tesisat Türü</th><th>Tutar</th><th>İşlem</th></tr></thead>
        <tbody>${rows || `<tr><td colspan="3">${document.getElementById('emptyState').innerHTML}</td></tr>`}</tbody>
      </table>
      <div class="total"><span>Genel Toplam</span><span>${money(grandTotal())}</span></div>
    </section>`;

  app.querySelectorAll('[data-open]').forEach((btn) => btn.onclick = () => navigate('work-details', { plumbingTypeId: btn.dataset.open }));
}

function renderSettings() {
  const plumbingOptions = state.db.plumbingTypes.map((p) => `<option value="${p.id}">${p.name}</option>`).join('');

  app.innerHTML = `<section class="card">
      <h2>Ayarlar</h2>
      <span class="badge">Tüm gruplar bu menüden eklenir/silinir.</span>
    </section>

    <section class="card">
      <h3>Tesisat Türü</h3>
      <form id="addPlumbing" class="inline">
        <input name="name" placeholder="Tesisat Türü Adı" required />
        <button class="primary" type="submit">Ekle</button>
      </form>
      <ul>${state.db.plumbingTypes.map((x) => `<li>${x.name} <button class="danger" data-del-plumbing="${x.id}">Sil</button></li>`).join('') || '<li>-</li>'}</ul>
    </section>

    <section class="card">
      <h3>İş Detayı Grubu</h3>
      <form id="addWD" class="inline">
        <select name="plumbingTypeId" required><option value="">Tesisat Türü Seçin</option>${plumbingOptions}</select>
        <input name="name" placeholder="İş Detayı Adı" required />
        <button class="primary" type="submit">Ekle</button>
      </form>
      <ul>${state.db.workDetails.map((x) => `<li>${x.name} <button class="danger" data-del-wd="${x.id}">Sil</button></li>`).join('') || '<li>-</li>'}</ul>
    </section>

    <section class="card grid">
      <div>
        <h3>Malzeme Şablonu</h3>
        <form id="addMatDef" class="inline"><input name="name" placeholder="Malzeme Adı" required /><button class="primary" type="submit">Ekle</button></form>
        <ul>${state.db.materialDefs.map((x) => `<li>${x.name} <button class="danger" data-del-mdef="${x.id}">Sil</button></li>`).join('') || '<li>-</li>'}</ul>
      </div>
      <div>
        <h3>Marka</h3>
        <form id="addBrand" class="inline"><input name="name" placeholder="Marka Adı" required /><button class="primary" type="submit">Ekle</button></form>
        <ul>${state.db.brands.map((x) => `<li>${x.name} <button class="danger" data-del-brand="${x.id}">Sil</button></li>`).join('') || '<li>-</li>'}</ul>
      </div>
    </section>`;

  formHandler('addPlumbing', (fd) => state.db.plumbingTypes.push({ id: uid('pt'), name: fd.get('name').toString().trim() }));
  formHandler('addWD', (fd) => state.db.workDetails.push({ id: uid('wd'), plumbingTypeId: fd.get('plumbingTypeId').toString(), name: fd.get('name').toString().trim() }));
  formHandler('addMatDef', (fd) => state.db.materialDefs.push({ id: uid('md'), name: fd.get('name').toString().trim() }));
  formHandler('addBrand', (fd) => state.db.brands.push({ id: uid('br'), name: fd.get('name').toString().trim() }));

  app.querySelectorAll('[data-del-plumbing]').forEach((b) => b.onclick = () => cascadeDeletePlumbing(b.dataset.delPlumbing));
  app.querySelectorAll('[data-del-wd]').forEach((b) => b.onclick = () => { state.db.workDetails = state.db.workDetails.filter((x) => x.id !== b.dataset.delWd); state.db.materialRows = state.db.materialRows.filter((x) => x.workDetailId !== b.dataset.delWd); saveDB(); renderSettings(); });
  app.querySelectorAll('[data-del-mdef]').forEach((b) => b.onclick = () => { state.db.materialDefs = state.db.materialDefs.filter((x) => x.id !== b.dataset.delMdef); saveDB(); renderSettings(); });
  app.querySelectorAll('[data-del-brand]').forEach((b) => b.onclick = () => { state.db.brands = state.db.brands.filter((x) => x.id !== b.dataset.delBrand); saveDB(); renderSettings(); });
}

function formHandler(id, fn) {
  const form = document.getElementById(id);
  form.onsubmit = (e) => {
    e.preventDefault();
    const fd = new FormData(form);
    fn(fd);
    saveDB();
    renderSettings();
  };
}

function cascadeDeletePlumbing(id) {
  const wdIds = state.db.workDetails.filter((w) => w.plumbingTypeId === id).map((w) => w.id);
  state.db.plumbingTypes = state.db.plumbingTypes.filter((x) => x.id !== id);
  state.db.workDetails = state.db.workDetails.filter((x) => x.plumbingTypeId !== id);
  state.db.materialRows = state.db.materialRows.filter((x) => !wdIds.includes(x.workDetailId));
  saveDB();
  renderSettings();
}

function renderWorkDetails(plumbingTypeId) {
  const pt = state.db.plumbingTypes.find((x) => x.id === plumbingTypeId);
  if (!pt) return goHome();
  const rows = workDetailsOf(plumbingTypeId).map((wd) => `<tr>
      <td>${wd.name}</td>
      <td>${money(workDetailTotal(wd.id))}</td>
      <td><button class="soft" data-open-wd="${wd.id}">Düzenle/İncele</button></td>
    </tr>`).join('');

  app.innerHTML = `<section class="card">
      <h2>${pt.name} / İş Detayları</h2>
      <table>
        <thead><tr><th>İş Detayı</th><th>Tutar</th><th>İşlem</th></tr></thead>
        <tbody>${rows || `<tr><td colspan="3">${document.getElementById('emptyState').innerHTML}</td></tr>`}</tbody>
      </table>
      <div class="total"><span>Tesisat Türü Toplamı</span><span>${money(plumbingTotal(pt.id))}</span></div>
      <div class="inline"><button class="soft" id="backHome">Geri</button></div>
    </section>`;

  document.getElementById('backHome').onclick = goHome;
  app.querySelectorAll('[data-open-wd]').forEach((btn) => btn.onclick = () => navigate('materials', { workDetailId: btn.dataset.openWd }));
}

function renderMaterials(workDetailId) {
  const wd = state.db.workDetails.find((x) => x.id === workDetailId);
  if (!wd) return goHome();

  const brandOptions = state.db.brands.map((b) => `<option value="${b.id}">${b.name}</option>`).join('');
  const matOptions = state.db.materialDefs.map((m) => `<option value="${m.name}"></option>`).join('');
  const rows = rowsOf(workDetailId);

  app.innerHTML = `<section class="card">
      <h2>${wd.name} / Malzeme Düzenleme</h2>
      <form id="addRow" class="grid">
        <input name="materialName" list="matDef" placeholder="Malzeme" required />
        <datalist id="matDef">${matOptions}</datalist>
        <select name="brandId" required><option value="">Marka seçin</option>${brandOptions}</select>
        <input name="quantity" type="number" min="0" step="0.01" placeholder="Adet" required />
        <input name="listPrice" type="number" min="0" step="0.01" placeholder="Liste Fiyatı" required />
        <input name="discount" type="number" min="0" max="100" step="0.01" placeholder="İskonto %" required />
        <input name="laborUnitPrice" type="number" min="0" step="0.01" placeholder="İşçilik Birim Fiyat" required />
        <button class="primary" type="submit">Satır Ekle</button>
      </form>
    </section>

    <section class="card">
      <h3>Satırlar</h3>
      <table>
        <thead>
          <tr><th>Malzeme</th><th>Marka</th><th>Adet</th><th>Liste</th><th>İskonto</th><th>Birim</th><th>Malzeme Top.</th><th>İşçilik Birim</th><th>İşçilik Top.</th><th>Genel Top.</th><th></th></tr>
        </thead>
        <tbody>
          ${rows.map((r) => {
            const c = calcRow(r);
            const b = state.db.brands.find((x) => x.id === r.brandId)?.name || '-';
            return `<tr>
              <td>${r.materialName}</td><td>${b}</td><td>${r.quantity}</td><td>${money(r.listPrice)}</td><td>%${Number(r.discount).toFixed(2)}</td>
              <td>${money(c.unit)}</td><td>${money(c.materialTotal)}</td><td>${money(r.laborUnitPrice)}</td><td>${money(c.laborTotal)}</td><td><b>${money(c.total)}</b></td>
              <td><button class="danger" data-del-row="${r.id}">Sil</button></td>
            </tr>`;
          }).join('') || `<tr><td colspan="11">${document.getElementById('emptyState').innerHTML}</td></tr>`}
        </tbody>
      </table>
      <div class="total"><span>İş Detayı Toplamı</span><span>${money(workDetailTotal(workDetailId))}</span></div>
      <div class="inline"><button class="soft" id="backWDS">İş Detaylarına Dön</button></div>
    </section>`;

  document.getElementById('addRow').onsubmit = (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    state.db.materialRows.push({
      id: uid('row'),
      workDetailId,
      materialName: fd.get('materialName').toString().trim(),
      brandId: fd.get('brandId').toString(),
      quantity: Number(fd.get('quantity')),
      listPrice: Number(fd.get('listPrice')),
      discount: Number(fd.get('discount')),
      laborUnitPrice: Number(fd.get('laborUnitPrice')),
    });
    saveDB();
    renderMaterials(workDetailId);
  };

  document.getElementById('backWDS').onclick = () => navigate('work-details', { plumbingTypeId: wd.plumbingTypeId });
  app.querySelectorAll('[data-del-row]').forEach((b) => b.onclick = () => {
    state.db.materialRows = state.db.materialRows.filter((x) => x.id !== b.dataset.delRow);
    saveDB();
    renderMaterials(workDetailId);
  });
}

function getDemoData() {
  const p1 = { id: uid('pt'), name: 'Temiz Su Tesisatı' };
  const wd1 = { id: uid('wd'), plumbingTypeId: p1.id, name: 'Zemin Kat Hatları' };
  const br = { id: uid('br'), name: 'ECA' };
  return {
    plumbingTypes: [p1],
    workDetails: [wd1],
    brands: [br],
    materialDefs: [{ id: uid('md'), name: 'PPRC Boru 20 mm' }],
    materialRows: [{ id: uid('row'), workDetailId: wd1.id, materialName: 'PPRC Boru 20 mm', brandId: br.id, quantity: 10, listPrice: 100, discount: 10, laborUnitPrice: 30 }],
  };
}

render();
