const storageKey = 'teklif_hazirlama_data_v1';

const state = loadState();
let currentGroupId = null;
let currentDetailId = null;

const views = {
  main: document.getElementById('mainView'),
  group: document.getElementById('groupView'),
  detail: document.getElementById('detailView'),
  materials: document.getElementById('materialsView'),
};

document.getElementById('navMain').addEventListener('click', () => showMainView());
document.getElementById('navMaterials').addEventListener('click', () => showMaterialsView());

showMainView();

function loadState() {
  const raw = localStorage.getItem(storageKey);
  if (!raw) {
    return {
      plumbingGroups: [],
      jobDetails: [],
      materialCatalog: [],
      lineItemsByDetail: {},
    };
  }
  return JSON.parse(raw);
}

function saveState() {
  localStorage.setItem(storageKey, JSON.stringify(state));
}

function uid(prefix) {
  return `${prefix}_${Math.random().toString(36).slice(2, 9)}`;
}

function showView(name) {
  Object.values(views).forEach((v) => v.classList.remove('active'));
  views[name].classList.add('active');
}

function formatMoney(value) {
  return Number(value || 0).toLocaleString('tr-TR', {
    style: 'currency',
    currency: 'TRY',
  });
}

function calcMaterialUnitPrice(catalogMaterial) {
  return catalogMaterial.listPrice * (1 - catalogMaterial.discount / 100);
}

function getDetailTotal(detailId) {
  const lines = state.lineItemsByDetail[detailId] || [];
  return lines.reduce((sum, line) => {
    const catalog = state.materialCatalog.find((m) => m.id === line.catalogMaterialId);
    if (!catalog) return sum;
    const unitMaterial = calcMaterialUnitPrice(catalog);
    const materialTotal = unitMaterial * line.quantity;
    const laborTotal = line.laborUnitPrice * line.quantity;
    return sum + materialTotal + laborTotal;
  }, 0);
}

function getGroupTotal(groupId) {
  return state.jobDetails
    .filter((d) => d.groupId === groupId)
    .reduce((sum, d) => sum + getDetailTotal(d.id), 0);
}

function showMainView() {
  showView('main');

  const rows = state.plumbingGroups
    .map(
      (g) => `
      <tr>
        <td>${g.name}</td>
        <td class="right">${formatMoney(getGroupTotal(g.id))}</td>
        <td>${g.description || ''}</td>
        <td>
          <div class="actions">
            <button data-open-group="${g.id}">Aç</button>
            <button data-edit-group="${g.id}">Düzenle</button>
            <button class="danger" data-delete-group="${g.id}">Sil</button>
          </div>
        </td>
      </tr>`,
    )
    .join('');

  views.main.innerHTML = `
    <div class="card">
      <h2>Ana Form</h2>
      <p class="small">Sadece Tesisat Grubu, Tutarı ve Detayı görüntülenir.</p>
      <form id="groupForm" class="grid">
        <label>Tesisat Grubu
          <input required name="name" placeholder="Örn. Sıhhi" />
        </label>
        <label>Detay
          <input name="description" placeholder="Kısa açıklama" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Tesisat Grubu Ekle</button>
        </label>
      </form>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr>
            <th>Tesisat Grubu</th>
            <th class="right">Tutar</th>
            <th>Detay</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          ${rows || '<tr><td colspan="4">Henüz tesisat grubu yok.</td></tr>'}
        </tbody>
      </table>
    </div>
  `;

  const groupForm = document.getElementById('groupForm');
  groupForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const formData = new FormData(groupForm);
    state.plumbingGroups.push({
      id: uid('grp'),
      name: String(formData.get('name')).trim(),
      description: String(formData.get('description') || '').trim(),
    });
    saveState();
    showMainView();
  });

  views.main.querySelectorAll('[data-open-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      currentGroupId = btn.dataset.openGroup;
      showGroupView();
    });
  });

  views.main.querySelectorAll('[data-edit-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const grp = state.plumbingGroups.find((g) => g.id === btn.dataset.editGroup);
      const name = prompt('Tesisat Grubu adı', grp.name);
      if (name === null) return;
      const detail = prompt('Detay', grp.description || '');
      if (detail === null) return;
      grp.name = name.trim() || grp.name;
      grp.description = detail.trim();
      saveState();
      showMainView();
    });
  });

  views.main.querySelectorAll('[data-delete-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const groupId = btn.dataset.deleteGroup;
      const detailIds = state.jobDetails.filter((d) => d.groupId === groupId).map((d) => d.id);
      state.plumbingGroups = state.plumbingGroups.filter((g) => g.id !== groupId);
      state.jobDetails = state.jobDetails.filter((d) => d.groupId !== groupId);
      state.materialCatalog = state.materialCatalog.filter((m) => m.groupId !== groupId);
      detailIds.forEach((id) => delete state.lineItemsByDetail[id]);
      saveState();
      showMainView();
    });
  });
}

function showGroupView() {
  const group = state.plumbingGroups.find((g) => g.id === currentGroupId);
  if (!group) return showMainView();

  showView('group');
  const details = state.jobDetails.filter((d) => d.groupId === group.id);

  const rows = details
    .map(
      (d) => `
      <tr>
        <td>${d.name}</td>
        <td class="right">${formatMoney(getDetailTotal(d.id))}</td>
        <td>${d.description || ''}</td>
        <td>
          <div class="actions">
            <button data-open-detail="${d.id}">İş Detayına Gir</button>
            <button data-edit-detail="${d.id}">Düzenle</button>
            <button class="danger" data-delete-detail="${d.id}">Sil</button>
          </div>
        </td>
      </tr>`,
    )
    .join('');

  views.group.innerHTML = `
    <div class="card">
      <div class="actions"><button id="backToMain">← Ana Forma Dön</button></div>
      <h2>${group.name} Tesisat Grubu</h2>
      <p class="small">İş Detayı, toplam tutar ve detay görüntülenir.</p>
      <form id="detailForm" class="grid">
        <label>İş Detayı
          <input name="name" required placeholder="Örn. Vitrifiye" />
        </label>
        <label>Detay
          <input name="description" placeholder="Açıklama" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">İş Detayı Ekle</button>
        </label>
      </form>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr><th>İş Detayı</th><th class="right">Toplam Tutar</th><th>Detay</th><th>İşlem</th></tr>
        </thead>
        <tbody>
          ${rows || '<tr><td colspan="4">Bu grupta iş detayı yok.</td></tr>'}
        </tbody>
        <tfoot>
          <tr><td><b>Genel Toplam</b></td><td class="right total">${formatMoney(getGroupTotal(group.id))}</td><td colspan="2"></td></tr>
        </tfoot>
      </table>
    </div>
  `;

  document.getElementById('backToMain').addEventListener('click', showMainView);
  const detailForm = document.getElementById('detailForm');
  detailForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(detailForm);
    state.jobDetails.push({
      id: uid('det'),
      groupId: group.id,
      name: String(fd.get('name')).trim(),
      description: String(fd.get('description') || '').trim(),
    });
    saveState();
    showGroupView();
  });

  views.group.querySelectorAll('[data-open-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      currentDetailId = btn.dataset.openDetail;
      showDetailView();
    });
  });

  views.group.querySelectorAll('[data-edit-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const det = state.jobDetails.find((d) => d.id === btn.dataset.editDetail);
      const name = prompt('İş Detayı adı', det.name);
      if (name === null) return;
      const desc = prompt('Detay', det.description || '');
      if (desc === null) return;
      det.name = name.trim() || det.name;
      det.description = desc.trim();
      saveState();
      showGroupView();
    });
  });

  views.group.querySelectorAll('[data-delete-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const id = btn.dataset.deleteDetail;
      state.jobDetails = state.jobDetails.filter((d) => d.id !== id);
      state.materialCatalog = state.materialCatalog.filter((m) => m.jobDetailId !== id);
      delete state.lineItemsByDetail[id];
      saveState();
      showGroupView();
    });
  });
}

function showDetailView() {
  const detail = state.jobDetails.find((d) => d.id === currentDetailId);
  if (!detail) return showGroupView();

  const group = state.plumbingGroups.find((g) => g.id === detail.groupId);
  const catalogForDetail = state.materialCatalog.filter((m) => m.jobDetailId === detail.id);
  const lines = state.lineItemsByDetail[detail.id] || [];

  showView('detail');

  const rowHtml = lines
    .map((line) => {
      const catalog = state.materialCatalog.find((m) => m.id === line.catalogMaterialId);
      if (!catalog) return '';
      const unit = calcMaterialUnitPrice(catalog);
      const materialTotal = line.quantity * unit;
      const laborTotal = line.quantity * line.laborUnitPrice;
      return `
      <tr>
        <td>${catalog.name}</td>
        <td>${catalog.brand}</td>
        <td class="right">${line.quantity}</td>
        <td class="right">${formatMoney(catalog.listPrice)}</td>
        <td class="right">%${catalog.discount}</td>
        <td class="right">${formatMoney(unit)}</td>
        <td class="right">${formatMoney(line.laborUnitPrice)}</td>
        <td class="right">${formatMoney(materialTotal)}</td>
        <td class="right">${formatMoney(laborTotal)}</td>
        <td class="right">${formatMoney(materialTotal + laborTotal)}</td>
        <td><button class="danger" data-delete-line="${line.id}">Sil</button></td>
      </tr>`;
    })
    .join('');

  views.detail.innerHTML = `
    <div class="card">
      <div class="actions"><button id="backToGroup">← ${group.name} grubuna dön</button></div>
      <h2>${detail.name} İş Detayı</h2>
      <p class="small">Malzeme, marka, adet, liste fiyat, iskonto ve işçilikten toplam tutar otomatik hesaplanır.</p>
      <form id="lineForm" class="grid">
        <label>Malzeme / Marka
          <select name="catalogMaterialId" required>
            <option value="">Seçiniz</option>
            ${catalogForDetail
              .map(
                (m) =>
                  `<option value="${m.id}">${m.name} - ${m.brand} (${formatMoney(calcMaterialUnitPrice(m))})</option>`,
              )
              .join('')}
          </select>
        </label>
        <label>Adet
          <input type="number" step="1" min="1" name="quantity" required value="1" />
        </label>
        <label>İşçilik Birim Fiyatı
          <input type="number" step="0.01" min="0" name="laborUnitPrice" required value="0" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Satır Ekle</button>
        </label>
      </form>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr>
            <th>Malzeme</th><th>Marka</th><th class="right">Adet</th><th class="right">Liste Fiyatı</th><th class="right">İskonto</th><th class="right">Birim Fiyat</th><th class="right">İşçilik Birim</th><th class="right">Malzeme Tutarı</th><th class="right">İşçilik Tutarı</th><th class="right">Toplam</th><th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          ${rowHtml || '<tr><td colspan="11">Henüz satır yok.</td></tr>'}
        </tbody>
        <tfoot>
          <tr><td colspan="9" class="right"><b>Genel Toplam</b></td><td class="right total">${formatMoney(getDetailTotal(detail.id))}</td><td></td></tr>
        </tfoot>
      </table>
    </div>
  `;

  document.getElementById('backToGroup').addEventListener('click', showGroupView);

  document.getElementById('lineForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const line = {
      id: uid('line'),
      catalogMaterialId: String(fd.get('catalogMaterialId')),
      quantity: Number(fd.get('quantity')),
      laborUnitPrice: Number(fd.get('laborUnitPrice')),
    };
    if (!state.lineItemsByDetail[detail.id]) state.lineItemsByDetail[detail.id] = [];
    state.lineItemsByDetail[detail.id].push(line);
    saveState();
    showDetailView();
  });

  views.detail.querySelectorAll('[data-delete-line]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      state.lineItemsByDetail[detail.id] = (state.lineItemsByDetail[detail.id] || []).filter(
        (l) => l.id !== btn.dataset.deleteLine,
      );
      saveState();
      showDetailView();
    });
  });
}

function showMaterialsView() {
  showView('materials');

  const groupOptions = state.plumbingGroups
    .map((g) => `<option value="${g.id}">${g.name}</option>`)
    .join('');
  const allRows = state.materialCatalog
    .map(
      (m) => {
        const grp = state.plumbingGroups.find((g) => g.id === m.groupId);
        const det = state.jobDetails.find((d) => d.id === m.jobDetailId);
        return `<tr>
          <td>${grp?.name || '-'}</td>
          <td>${det?.name || '-'}</td>
          <td>${m.name}</td>
          <td>${m.brand}</td>
          <td class="right">${formatMoney(m.listPrice)}</td>
          <td class="right">%${m.discount}</td>
          <td class="right">${formatMoney(calcMaterialUnitPrice(m))}</td>
          <td><button class="danger" data-delete-material="${m.id}">Sil</button></td>
        </tr>`;
      },
    )
    .join('');

  views.materials.innerHTML = `
    <div class="card">
      <h2>Malzeme Yönetimi</h2>
      <p class="small">Tesisat Grubu ve İş Detayı seçip malzeme, marka, fiyat ve iskonto ile kayıt açın.</p>
      <form id="materialForm" class="grid">
        <label>Tesisat Grubu
          <select id="materialGroupSelect" name="groupId" required>
            <option value="">Seçiniz</option>
            ${groupOptions}
          </select>
        </label>
        <label>İş Detayı
          <select id="materialDetailSelect" name="jobDetailId" required>
            <option value="">Önce tesisat grubu seçin</option>
          </select>
        </label>
        <label>Malzeme Adı
          <input name="name" required placeholder="Örn. Lavabo" />
        </label>
        <label>Marka
          <input name="brand" required placeholder="Örn. ECA" />
        </label>
        <label>Liste Fiyatı
          <input type="number" step="0.01" min="0" name="listPrice" required />
        </label>
        <label>İskonto (%)
          <input type="number" step="0.01" min="0" max="100" name="discount" required value="0" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Ekle</button>
        </label>
      </form>
    </div>

    <div class="card">
      <h3>Kayıtlı Malzemeler</h3>
      <table>
        <thead>
          <tr>
            <th>Tesisat Grubu</th><th>İş Detayı</th><th>Malzeme</th><th>Marka</th><th class="right">Liste Fiyatı</th><th class="right">İskonto</th><th class="right">Birim Fiyat</th><th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          ${allRows || '<tr><td colspan="8">Kayıt yok.</td></tr>'}
        </tbody>
      </table>
    </div>
  `;

  const groupSelect = document.getElementById('materialGroupSelect');
  const detailSelect = document.getElementById('materialDetailSelect');

  const fillDetails = () => {
    const details = state.jobDetails.filter((d) => d.groupId === groupSelect.value);
    detailSelect.innerHTML = `
      <option value="">Seçiniz</option>
      ${details.map((d) => `<option value="${d.id}">${d.name}</option>`).join('')}
    `;
  };
  groupSelect.addEventListener('change', fillDetails);

  document.getElementById('materialForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const groupId = String(fd.get('groupId'));
    const jobDetailId = String(fd.get('jobDetailId'));
    const name = String(fd.get('name')).trim();
    const brand = String(fd.get('brand')).trim();

    const duplicate = state.materialCatalog.some(
      (m) => m.name.toLowerCase() === name.toLowerCase() && m.brand.toLowerCase() === brand.toLowerCase(),
    );

    if (duplicate) {
      alert('Aynı malzeme adı ve marka ile ikinci bir kayıt eklenemez.');
      return;
    }

    state.materialCatalog.push({
      id: uid('mat'),
      groupId,
      jobDetailId,
      name,
      brand,
      listPrice: Number(fd.get('listPrice')),
      discount: Number(fd.get('discount')),
    });

    saveState();
    showMaterialsView();
  });

  views.materials.querySelectorAll('[data-delete-material]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const materialId = btn.dataset.deleteMaterial;
      state.materialCatalog = state.materialCatalog.filter((m) => m.id !== materialId);
      Object.keys(state.lineItemsByDetail).forEach((detailId) => {
        state.lineItemsByDetail[detailId] = state.lineItemsByDetail[detailId].filter(
          (line) => line.catalogMaterialId !== materialId,
        );
      });
      saveState();
      showMaterialsView();
    });
  });
}
