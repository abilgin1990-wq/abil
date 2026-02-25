const STORAGE_KEY = 'teklif_hazirlama_v2';

const state = {
  groups: [],
  materialCatalog: [],
  currentGroupId: null,
  currentDetailId: null,
};

const views = {
  main: document.getElementById('mainView'),
  group: document.getElementById('groupView'),
  detail: document.getElementById('detailView'),
  management: document.getElementById('managementView'),
};

const el = {
  groupsTableBody: document.getElementById('groupsTableBody'),
  detailsTableBody: document.getElementById('detailsTableBody'),
  linesTableBody: document.getElementById('linesTableBody'),
  detailGrandTotal: document.getElementById('detailGrandTotal'),
  managementTableBody: document.getElementById('managementTableBody'),
  groupTitle: document.getElementById('groupTitle'),
  detailTitle: document.getElementById('detailTitle'),
  groupNameInput: document.getElementById('groupNameInput'),
  detailNameInput: document.getElementById('detailNameInput'),
  manageGroupSelect: document.getElementById('manageGroupSelect'),
  manageDetailSelect: document.getElementById('manageDetailSelect'),
  manageCatalogSelect: document.getElementById('manageCatalogSelect'),
  detailCatalogSelect: document.getElementById('detailCatalogSelect'),
  catalogGroupSelect: document.getElementById('catalogGroupSelect'),
  catalogDetailSelect: document.getElementById('catalogDetailSelect'),
  catalogMaterialInput: document.getElementById('catalogMaterialInput'),
  catalogBrandInput: document.getElementById('catalogBrandInput'),
  catalogListPriceInput: document.getElementById('catalogListPriceInput'),
  catalogDiscountInput: document.getElementById('catalogDiscountInput'),
  catalogLaborInput: document.getElementById('catalogLaborInput'),
  catalogTableBody: document.getElementById('catalogTableBody'),
};

function generateId() {
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function formatTL(value) {
  return `${Number(value || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TL`;
}

function getGroupById(groupId) {
  return state.groups.find((g) => g.id === groupId);
}

function getDetailById(group, detailId) {
  return group.details.find((d) => d.id === detailId);
}

function getCatalogById(catalogId) {
  return state.materialCatalog.find((item) => item.id === catalogId);
}

function getCatalogForDetail(groupId, detailId) {
  return state.materialCatalog.filter((item) => item.groupId === groupId && item.detailId === detailId);
}

function computeLine(line) {
  const adet = Number(line.qty || 0);
  const liste = Number(line.listPrice || 0);
  const iskonto = Number(line.discount || 0);
  const iscilikBirim = Number(line.laborUnitPrice || 0);
  const birim = liste * (1 - iskonto / 100);
  const malzemeToplam = birim * adet;
  const iscilikToplam = iscilikBirim * adet;
  return {
    birim,
    malzemeToplam,
    iscilikToplam,
    satirToplam: malzemeToplam + iscilikToplam,
  };
}

function detailTotal(detail) {
  return detail.lines.reduce((sum, line) => sum + computeLine(line).satirToplam, 0);
}

function groupTotal(group) {
  return group.details.reduce((sum, detail) => sum + detailTotal(detail), 0);
}

function setView(viewName) {
  Object.values(views).forEach((v) => v.classList.remove('active'));
  views[viewName].classList.add('active');
}

function loadData() {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return;
  try {
    const parsed = JSON.parse(raw);
    if (Array.isArray(parsed.groups)) state.groups = parsed.groups;
    if (Array.isArray(parsed.materialCatalog)) state.materialCatalog = parsed.materialCatalog;
  } catch (error) {
    alert('Kaydedilmiş veri okunamadı, boş veri ile devam ediliyor.');
  }
}

function saveData() {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ groups: state.groups, materialCatalog: state.materialCatalog }));
  alert('Kayıt başarılı.');
}

function renderMain() {
  el.groupsTableBody.innerHTML = '';
  state.groups.forEach((group) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${group.name}</td>
      <td>${formatTL(groupTotal(group))}</td>
      <td><button data-action="open-group" data-id="${group.id}">Detaya Git</button></td>
      <td>
        <button data-action="edit-group" data-id="${group.id}">Düzenle</button>
        <button data-action="delete-group" data-id="${group.id}">Sil</button>
      </td>
    `;
    el.groupsTableBody.appendChild(tr);
  });

  renderCatalogPanel();
}

function renderCatalogPanel() {
  fillCatalogSelectors();
  el.catalogTableBody.innerHTML = '';
  state.materialCatalog.forEach((item) => {
    const group = getGroupById(item.groupId);
    const detail = group ? getDetailById(group, item.detailId) : null;
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${group ? group.name : '-'}</td>
      <td>${detail ? detail.name : '-'}</td>
      <td>${item.material}</td>
      <td>${item.brand}</td>
      <td>${formatTL(item.listPrice)}</td>
      <td>${item.discount}</td>
      <td>${formatTL(item.laborUnitPrice)}</td>
      <td>
        <button data-action="edit-catalog" data-id="${item.id}">Düzenle</button>
        <button data-action="delete-catalog" data-id="${item.id}">Sil</button>
      </td>
    `;
    el.catalogTableBody.appendChild(tr);
  });
}

function renderGroup() {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  el.groupTitle.textContent = `Tesisat Grubu: ${group.name}`;
  el.detailsTableBody.innerHTML = '';
  group.details.forEach((detail) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${detail.name}</td>
      <td>${formatTL(detailTotal(detail))}</td>
      <td><button data-action="open-detail" data-id="${detail.id}">Detaya Git</button></td>
      <td>
        <button data-action="edit-detail" data-id="${detail.id}">Düzenle</button>
        <button data-action="delete-detail" data-id="${detail.id}">Sil</button>
      </td>
    `;
    el.detailsTableBody.appendChild(tr);
  });
}

function fillDetailCatalogSelect() {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;

  el.detailCatalogSelect.innerHTML = '';
  const options = getCatalogForDetail(group.id, detail.id);
  options.forEach((item) => {
    const opt = document.createElement('option');
    opt.value = item.id;
    opt.textContent = `${item.material} / ${item.brand} (${formatTL(item.listPrice)})`;
    el.detailCatalogSelect.appendChild(opt);
  });
}

function renderDetail() {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;

  el.detailTitle.textContent = `İş Detayı: ${detail.name} (${group.name})`;
  fillDetailCatalogSelect();
  el.linesTableBody.innerHTML = '';

  detail.lines.forEach((line) => {
    const c = computeLine(line);
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${line.material}</td>
      <td>${line.brand}</td>
      <td><input class="small-input" type="number" min="0" step="1" value="${line.qty}" data-field="qty" data-id="${line.id}"></td>
      <td><input class="small-input" type="number" min="0" step="0.01" value="${line.listPrice}" data-field="listPrice" data-id="${line.id}"></td>
      <td><input class="small-input" type="number" min="0" step="0.01" value="${line.discount}" data-field="discount" data-id="${line.id}"></td>
      <td>${formatTL(c.birim)}</td>
      <td><input class="small-input" type="number" min="0" step="0.01" value="${line.laborUnitPrice}" data-field="laborUnitPrice" data-id="${line.id}"></td>
      <td>${formatTL(c.iscilikToplam)}</td>
      <td>${formatTL(c.malzemeToplam)}</td>
      <td>${formatTL(c.satirToplam)}</td>
      <td><button data-action="delete-line" data-id="${line.id}">Sil</button></td>
    `;
    el.linesTableBody.appendChild(tr);
  });

  el.detailGrandTotal.textContent = formatTL(detailTotal(detail));
}

function fillManagementSelectors() {
  el.manageGroupSelect.innerHTML = '';
  state.groups.forEach((group) => {
    const opt = document.createElement('option');
    opt.value = group.id;
    opt.textContent = group.name;
    el.manageGroupSelect.appendChild(opt);
  });
  fillManageDetails();
}

function fillManageDetails() {
  el.manageDetailSelect.innerHTML = '';
  const group = getGroupById(el.manageGroupSelect.value);
  if (!group) return;
  group.details.forEach((detail) => {
    const opt = document.createElement('option');
    opt.value = detail.id;
    opt.textContent = detail.name;
    el.manageDetailSelect.appendChild(opt);
  });
  fillManageCatalog();
}

function fillManageCatalog() {
  el.manageCatalogSelect.innerHTML = '';
  const options = getCatalogForDetail(el.manageGroupSelect.value, el.manageDetailSelect.value);
  options.forEach((item) => {
    const opt = document.createElement('option');
    opt.value = item.id;
    opt.textContent = `${item.material} / ${item.brand}`;
    el.manageCatalogSelect.appendChild(opt);
  });
}

function renderManagement() {
  fillManagementSelectors();
  el.managementTableBody.innerHTML = '';
  state.groups.forEach((group) => {
    group.details.forEach((detail) => {
      detail.lines.forEach((line) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${group.name}</td>
          <td>${detail.name}</td>
          <td>${line.material}</td>
          <td>${line.brand}</td>
          <td>${formatTL(line.listPrice)}</td>
          <td>${line.discount}</td>
          <td>${formatTL(line.laborUnitPrice)}</td>
          <td>
            <button data-action="manage-edit-line" data-group-id="${group.id}" data-detail-id="${detail.id}" data-line-id="${line.id}">Düzenle</button>
            <button data-action="manage-delete-line" data-group-id="${group.id}" data-detail-id="${detail.id}" data-line-id="${line.id}">Sil</button>
          </td>
        `;
        el.managementTableBody.appendChild(tr);
      });
    });
  });
}

function fillCatalogSelectors() {
  el.catalogGroupSelect.innerHTML = '';
  state.groups.forEach((group) => {
    const opt = document.createElement('option');
    opt.value = group.id;
    opt.textContent = group.name;
    el.catalogGroupSelect.appendChild(opt);
  });

  fillCatalogDetailSelector();
}

function fillCatalogDetailSelector() {
  el.catalogDetailSelect.innerHTML = '';
  const group = getGroupById(el.catalogGroupSelect.value);
  if (!group) return;
  group.details.forEach((detail) => {
    const opt = document.createElement('option');
    opt.value = detail.id;
    opt.textContent = detail.name;
    el.catalogDetailSelect.appendChild(opt);
  });
}

function addLineFromCatalog(detail, catalogId) {
  const catalog = getCatalogById(catalogId);
  if (!catalog) {
    alert('Lütfen katalogdan bir malzeme seçin.');
    return;
  }
  detail.lines.push({
    id: generateId(),
    material: catalog.material,
    brand: catalog.brand,
    qty: 1,
    listPrice: Number(catalog.listPrice),
    discount: Number(catalog.discount),
    laborUnitPrice: Number(catalog.laborUnitPrice),
  });
}

function bootstrapSampleIfEmpty() {
  if (state.groups.length > 0) return;

  const groupId = generateId();
  const detailId = generateId();
  const catalogId = generateId();

  state.groups.push({
    id: groupId,
    name: 'Sıhhi Tesisat',
    details: [
      {
        id: detailId,
        name: 'Vitrifiye',
        lines: [
          {
            id: generateId(),
            catalogId,
            material: 'Lavabo',
            brand: 'ECA',
            qty: 1,
            listPrice: 2500,
            discount: 10,
            laborUnitPrice: 350,
          },
        ],
      },
    ],
  });

  state.materialCatalog.push({
    id: catalogId,
    groupId,
    detailId,
    material: 'Lavabo',
    brand: 'ECA',
    listPrice: 2500,
    discount: 10,
    laborUnitPrice: 350,
  });
}

document.getElementById('addGroupBtn').addEventListener('click', () => {
  const name = el.groupNameInput.value.trim();
  if (!name) return;
  state.groups.push({ id: generateId(), name, details: [] });
  el.groupNameInput.value = '';
  renderMain();
  renderManagement();
});

document.getElementById('addCatalogBtn').addEventListener('click', () => {
  const groupId = el.catalogGroupSelect.value;
  const detailId = el.catalogDetailSelect.value;
  const material = el.catalogMaterialInput.value.trim();
  const brand = el.catalogBrandInput.value.trim();
  const listPrice = Number(el.catalogListPriceInput.value || 0);
  const discount = Number(el.catalogDiscountInput.value || 0);
  const laborUnitPrice = Number(el.catalogLaborInput.value || 0);

  if (!groupId || !detailId || !material || !brand) {
    alert('Lütfen grup, iş detayı, malzeme ve marka alanlarını doldurun.');
    return;
  }

  state.materialCatalog.push({
    id: generateId(),
    groupId,
    detailId,
    material,
    brand,
    listPrice,
    discount,
    laborUnitPrice,
  });

  el.catalogMaterialInput.value = '';
  el.catalogBrandInput.value = '';
  el.catalogListPriceInput.value = '';
  el.catalogDiscountInput.value = '';
  el.catalogLaborInput.value = '';

  renderMain();
  renderDetail();
  renderManagement();
});

document.getElementById('saveBtn').addEventListener('click', saveData);
document.getElementById('mainMenuBtn').addEventListener('click', () => {
  setView('main');
  renderMain();
});
document.getElementById('managementBtn').addEventListener('click', () => {
  setView('management');
  renderManagement();
});
document.getElementById('backToMainBtn').addEventListener('click', () => {
  setView('main');
  renderMain();
});
document.getElementById('backToGroupBtn').addEventListener('click', () => {
  setView('group');
  renderGroup();
});

document.getElementById('addDetailBtn').addEventListener('click', () => {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const name = el.detailNameInput.value.trim();
  if (!name) return;
  group.details.push({ id: generateId(), name, lines: [] });
  el.detailNameInput.value = '';
  renderGroup();
  renderMain();
  renderManagement();
});

document.getElementById('addLineBtn').addEventListener('click', () => {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;
  addLineFromCatalog(detail, el.detailCatalogSelect.value);
  renderDetail();
  renderGroup();
  renderMain();
  renderManagement();
});

document.getElementById('manageAddLineBtn').addEventListener('click', () => {
  const group = getGroupById(el.manageGroupSelect.value);
  if (!group) return;
  const detail = getDetailById(group, el.manageDetailSelect.value);
  if (!detail) return;
  addLineFromCatalog(detail, el.manageCatalogSelect.value);
  renderManagement();
  renderDetail();
  renderGroup();
  renderMain();
});

el.manageGroupSelect.addEventListener('change', fillManageDetails);
el.manageDetailSelect.addEventListener('change', fillManageCatalog);
el.catalogGroupSelect.addEventListener('change', fillCatalogDetailSelector);

el.groupsTableBody.addEventListener('click', (event) => {
  const btn = event.target.closest('button');
  if (!btn) return;
  const group = getGroupById(btn.dataset.id);
  if (!group) return;

  if (btn.dataset.action === 'open-group') {
    state.currentGroupId = group.id;
    setView('group');
    renderGroup();
  }
  if (btn.dataset.action === 'edit-group') {
    const next = prompt('Yeni grup adı:', group.name);
    if (next) {
      group.name = next;
      renderMain();
      renderGroup();
      renderManagement();
    }
  }
  if (btn.dataset.action === 'delete-group') {
    if (!confirm(`"${group.name}" grubunu silmek istiyor musunuz?`)) return;
    state.groups = state.groups.filter((g) => g.id !== group.id);
    state.materialCatalog = state.materialCatalog.filter((item) => item.groupId !== group.id);
    renderMain();
    renderManagement();
  }
});

el.catalogTableBody.addEventListener('click', (event) => {
  const btn = event.target.closest('button');
  if (!btn) return;
  const item = getCatalogById(btn.dataset.id);
  if (!item) return;

  if (btn.dataset.action === 'edit-catalog') {
    const material = prompt('Malzeme adı:', item.material);
    if (!material) return;
    const brand = prompt('Marka:', item.brand);
    if (!brand) return;
    const listPrice = Number(prompt('Liste fiyatı:', item.listPrice) || item.listPrice);
    const discount = Number(prompt('İskonto %:', item.discount) || item.discount);
    const laborUnitPrice = Number(prompt('İşçilik birim fiyatı:', item.laborUnitPrice) || item.laborUnitPrice);

    item.material = material;
    item.brand = brand;
    item.listPrice = listPrice;
    item.discount = discount;
    item.laborUnitPrice = laborUnitPrice;

    state.groups.forEach((group) => {
      group.details.forEach((detail) => {
        detail.lines.forEach((line) => {
          if (line.catalogId === item.id) {
            line.material = item.material;
            line.brand = item.brand;
            line.listPrice = item.listPrice;
            line.discount = item.discount;
            line.laborUnitPrice = item.laborUnitPrice;
          }
        });
      });
    });

    renderMain();
    renderDetail();
    renderGroup();
    renderManagement();
  }

  if (btn.dataset.action === 'delete-catalog') {
    if (!confirm('Katalog malzemesini silmek istiyor musunuz?')) return;
    state.materialCatalog = state.materialCatalog.filter((catalogItem) => catalogItem.id !== item.id);

    state.groups.forEach((group) => {
      group.details.forEach((detail) => {
        detail.lines = detail.lines.filter((line) => line.catalogId !== item.id);
      });
    });

    renderMain();
    renderDetail();
    renderGroup();
    renderManagement();
  }
});

el.detailsTableBody.addEventListener('click', (event) => {
  const btn = event.target.closest('button');
  if (!btn) return;
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, btn.dataset.id);
  if (!detail) return;

  if (btn.dataset.action === 'open-detail') {
    state.currentDetailId = detail.id;
    setView('detail');
    renderDetail();
  }
  if (btn.dataset.action === 'edit-detail') {
    const next = prompt('Yeni iş detayı adı:', detail.name);
    if (next) {
      detail.name = next;
      renderGroup();
      renderMain();
      renderManagement();
    }
  }
  if (btn.dataset.action === 'delete-detail') {
    if (!confirm(`"${detail.name}" iş detayını silmek istiyor musunuz?`)) return;
    group.details = group.details.filter((d) => d.id !== detail.id);
    state.materialCatalog = state.materialCatalog.filter((item) => item.detailId !== detail.id);
    renderGroup();
    renderMain();
    renderManagement();
  }
});

el.linesTableBody.addEventListener('input', (event) => {
  const input = event.target.closest('input');
  if (!input) return;
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;
  const line = detail.lines.find((item) => item.id === input.dataset.id);
  if (!line) return;
  line[input.dataset.field] = Number(input.value || 0);
  renderDetail();
  renderGroup();
  renderMain();
  renderManagement();
});

el.linesTableBody.addEventListener('click', (event) => {
  const btn = event.target.closest('button');
  if (!btn) return;
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;
  const line = detail.lines.find((item) => item.id === btn.dataset.id);
  if (!line) return;

  if (btn.dataset.action === 'delete-line') {
    if (!confirm(`"${line.material} - ${line.brand}" satırını silmek istiyor musunuz?`)) return;
    detail.lines = detail.lines.filter((item) => item.id !== line.id);
    renderDetail();
    renderGroup();
    renderMain();
    renderManagement();
  }
});

el.managementTableBody.addEventListener('click', (event) => {
  const btn = event.target.closest('button');
  if (!btn) return;

  const group = getGroupById(btn.dataset.groupId);
  if (!group) return;
  const detail = getDetailById(group, btn.dataset.detailId);
  if (!detail) return;
  const line = detail.lines.find((item) => item.id === btn.dataset.lineId);
  if (!line) return;

  if (btn.dataset.action === 'manage-edit-line') {
    const material = prompt('Malzeme adı:', line.material);
    if (!material) return;
    const brand = prompt('Marka:', line.brand);
    if (!brand) return;
    const listPrice = Number(prompt('Liste fiyatı:', line.listPrice) || line.listPrice);
    const discount = Number(prompt('İskonto %:', line.discount) || line.discount);
    const laborUnitPrice = Number(prompt('İşçilik birim fiyatı:', line.laborUnitPrice) || line.laborUnitPrice);

    line.material = material;
    line.brand = brand;
    line.listPrice = listPrice;
    line.discount = discount;
    line.laborUnitPrice = laborUnitPrice;

    renderManagement();
    renderDetail();
    renderGroup();
    renderMain();
  }

  if (btn.dataset.action === 'manage-delete-line') {
    if (!confirm('Seçili malzemeyi silmek istiyor musunuz?')) return;
    detail.lines = detail.lines.filter((lineItem) => lineItem.id !== btn.dataset.lineId);

    renderManagement();
    renderDetail();
    renderGroup();
    renderMain();
  }
});

loadData();
bootstrapSampleIfEmpty();
renderMain();
renderManagement();
