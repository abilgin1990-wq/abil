const STORAGE_KEY = 'teklif_hazirlama_v1';

const state = {
  groups: [],
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
    if (Array.isArray(parsed.groups)) {
      state.groups = parsed.groups;
    }
  } catch (error) {
    alert('Kaydedilmiş veri okunamadı, boş veri ile devam ediliyor.');
  }
}

function saveData() {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ groups: state.groups }));
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

function renderDetail() {
  const group = getGroupById(state.currentGroupId);
  if (!group) return;
  const detail = getDetailById(group, state.currentDetailId);
  if (!detail) return;

  el.detailTitle.textContent = `İş Detayı: ${detail.name} (${group.name})`;
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
      <td>
        <button data-action="edit-line" data-id="${line.id}">Düzenle</button>
        <button data-action="delete-line" data-id="${line.id}">Sil</button>
      </td>
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
          <td><button data-action="manage-delete-line" data-group-id="${group.id}" data-detail-id="${detail.id}" data-line-id="${line.id}">Sil</button></td>
        `;
        el.managementTableBody.appendChild(tr);
      });
    });
  });
}

function addLineToDetail(detail) {
  const material = prompt('Malzeme adı (örn. Lavabo):');
  if (!material) return;
  const brand = prompt('Marka (örn. ECA):');
  if (!brand) return;
  const listPrice = Number(prompt('Liste fiyatı:', '0') || 0);
  const discount = Number(prompt('İskonto %:', '0') || 0);
  const laborUnitPrice = Number(prompt('İşçilik birim fiyatı:', '0') || 0);
  detail.lines.push({
    id: generateId(),
    material,
    brand,
    qty: 1,
    listPrice,
    discount,
    laborUnitPrice,
  });
}

function bootstrapSampleIfEmpty() {
  if (state.groups.length > 0) return;
  state.groups.push({
    id: generateId(),
    name: 'Sıhhi Tesisat',
    details: [
      {
        id: generateId(),
        name: 'Vitrifiye',
        lines: [
          {
            id: generateId(),
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
}

document.getElementById('addGroupBtn').addEventListener('click', () => {
  const name = el.groupNameInput.value.trim();
  if (!name) return;
  state.groups.push({ id: generateId(), name, details: [] });
  el.groupNameInput.value = '';
  renderMain();
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
  addLineToDetail(detail);
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
  addLineToDetail(detail);
  renderManagement();
  renderDetail();
  renderGroup();
  renderMain();
});

el.manageGroupSelect.addEventListener('change', fillManageDetails);

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
    renderMain();
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

  if (btn.dataset.action === 'edit-line') {
    const material = prompt('Malzeme adı:', line.material);
    if (material) line.material = material;
    const brand = prompt('Marka:', line.brand);
    if (brand) line.brand = brand;
    renderDetail();
    renderGroup();
    renderMain();
    renderManagement();
  }
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
  if (btn.dataset.action !== 'manage-delete-line') return;

  const group = getGroupById(btn.dataset.groupId);
  if (!group) return;
  const detail = getDetailById(group, btn.dataset.detailId);
  if (!detail) return;

  if (!confirm('Seçili malzemeyi silmek istiyor musunuz?')) return;
  detail.lines = detail.lines.filter((line) => line.id !== btn.dataset.lineId);

  renderManagement();
  renderDetail();
  renderGroup();
  renderMain();
});

loadData();
bootstrapSampleIfEmpty();
renderMain();
renderManagement();
