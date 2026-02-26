const storageKey = 'teklif_hazirlama_data_v1';

const state = loadState();
let currentGroupId = null;
let currentDetailId = null;

const views = {
  quotes: document.getElementById('quotesView'),
  catalog: document.getElementById('catalogView'),
  main: document.getElementById('mainView'),
  group: document.getElementById('groupView'),
  detail: document.getElementById('detailView'),
  materials: document.getElementById('materialsView'),
};

document.getElementById('navQuotes').addEventListener('click', () => showQuotesView());
document.getElementById('navCatalog').addEventListener('click', () => showCatalogView());
document.getElementById('navMain').addEventListener('click', () => showMainView());
document.getElementById('navMaterials').addEventListener('click', () => showMaterialsView());

showQuotesView();

function loadState() {
  const raw = localStorage.getItem(storageKey);
  if (!raw) {
    return {
      proposals: [],
      activeProposalId: null,
      globalCatalog: { plumbingGroups: [], jobDetails: [], materialCatalog: [], exchangeRates: { usd: 1, eur: 1 } },
    };
  }

  const parsed = JSON.parse(raw);

  if (!parsed.proposals) {
    const migratedProposal = {
      id: uid('prp'),
      firmName: 'Varsayılan Firma',
      projectName: 'Varsayılan Proje',
      updatedAt: new Date().toISOString(),
      data: {
        plumbingGroups: parsed.plumbingGroups || [],
        jobDetails: parsed.jobDetails || [],
        materialCatalog: parsed.materialCatalog || [],
        lineItemsByDetail: parsed.lineItemsByDetail || {},
        exchangeRates: parsed.exchangeRates || { usd: 1, eur: 1 },
      },
    };

    return {
      proposals: [migratedProposal],
      activeProposalId: migratedProposal.id,
      globalCatalog: {
        plumbingGroups: migratedProposal.data.plumbingGroups || [],
        jobDetails: migratedProposal.data.jobDetails || [],
        materialCatalog: migratedProposal.data.materialCatalog || [],
        exchangeRates: migratedProposal.data.exchangeRates || { usd: 1, eur: 1 },
      },
    };
  }

  parsed.proposals.forEach((proposal) => {
    if (!proposal.data) proposal.data = {};
    if (!proposal.data.plumbingGroups) proposal.data.plumbingGroups = [];
    if (!proposal.data.jobDetails) proposal.data.jobDetails = [];
    if (!proposal.data.materialCatalog) proposal.data.materialCatalog = [];
    if (!proposal.data.lineItemsByDetail) proposal.data.lineItemsByDetail = {};
    if (!proposal.data.exchangeRates) proposal.data.exchangeRates = { usd: 1, eur: 1 };
    if (typeof proposal.data.exchangeRates.usd !== 'number') proposal.data.exchangeRates.usd = 1;
    if (typeof proposal.data.exchangeRates.eur !== 'number') proposal.data.exchangeRates.eur = 1;
    if (!proposal.updatedAt) proposal.updatedAt = new Date().toISOString();
  });

  if (!parsed.activeProposalId && parsed.proposals[0]) {
    parsed.activeProposalId = parsed.proposals[0].id;
  }
  if (!parsed.globalCatalog) {
    const first = parsed.proposals[0];
    parsed.globalCatalog = {
      plumbingGroups: first?.data?.plumbingGroups || [],
      jobDetails: first?.data?.jobDetails || [],
      materialCatalog: first?.data?.materialCatalog || [],
      exchangeRates: first?.data?.exchangeRates || { usd: 1, eur: 1 },
    };
  }
  if (!parsed.globalCatalog.exchangeRates) parsed.globalCatalog.exchangeRates = { usd: 1, eur: 1 };

  return parsed;
}
function saveState() {
  const active = getActiveProposal();
  if (active) {
    active.updatedAt = new Date().toISOString();
  }
  localStorage.setItem(storageKey, JSON.stringify(state));
}

function getActiveProposal() {
  return state.proposals.find((p) => p.id === state.activeProposalId) || null;
}

function getData() {
  const proposal = getActiveProposal();
  if (!proposal) return null;
  return proposal.data;
}

function getGlobalCatalog() {
  if (!state.globalCatalog) {
    state.globalCatalog = { plumbingGroups: [], jobDetails: [], materialCatalog: [], exchangeRates: { usd: 1, eur: 1 } };
  }
  return state.globalCatalog;
}

function getMaterialCatalog() {
  return getGlobalCatalog().materialCatalog;
}

function createEmptyProposalData() {
  return {
    plumbingGroups: [],
    jobDetails: [],
    lineItemsByDetail: {},
  };
}

function showQuotesView() {
  showView('quotes');

  const rows = state.proposals
    .map((p) => {
      const total = getProposalTotal(p);
      return `<tr>
        <td>${p.firmName}</td>
        <td>${p.projectName}</td>
        <td class="right">${formatMoney(total)}</td>
        <td>${new Date(p.updatedAt).toLocaleString('tr-TR')}</td>
        <td><button data-open-proposal="${p.id}">Teklifi Gör</button></td>
      </tr>`;
    })
    .join('');

  views.quotes.innerHTML = `
    <div class="card">
      <h2>Teklifler</h2>
      <form id="proposalForm" class="grid">
        <label>Teklifin Verileceği Firma
          <input name="firmName" required placeholder="Firma adı" />
        </label>
        <label>Proje İsmi
          <input name="projectName" required placeholder="Proje adı" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Teklif Ekle</button>
        </label>
      </form>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr><th>Firma</th><th>Proje</th><th class="right">Toplam Tutar</th><th>Son Güncelleme</th><th>İşlem</th></tr>
        </thead>
        <tbody>
          ${rows || '<tr><td colspan="5">Henüz teklif yok.</td></tr>'}
        </tbody>
      </table>
    </div>
  `;

  document.getElementById('proposalForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const proposal = {
      id: uid('prp'),
      firmName: String(fd.get('firmName')).trim(),
      projectName: String(fd.get('projectName')).trim(),
      updatedAt: new Date().toISOString(),
      data: createEmptyProposalData(),
    };
    state.proposals.push(proposal);
    state.activeProposalId = proposal.id;
    saveState();
    showQuotesView();
  });

  views.quotes.querySelectorAll('[data-open-proposal]').forEach((btn) => {
    btn.addEventListener('click', () => {
      state.activeProposalId = btn.dataset.openProposal;
      showMainView();
    });
  });
}

function getProposalTotal(proposal) {
  const data = proposal.data;
  const detailTotal = (detailId) => {
    const lines = data.lineItemsByDetail[detailId] || [];
    return lines.reduce((sum, line) => {
      const pricing = getLinePricing(line);
      if (!pricing) return sum;
      return sum + pricing.materialTotal + pricing.laborTotal;
    }, 0);
  };

  return data.jobDetails.reduce((sum, d) => sum + detailTotal(d.id), 0);
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
  const listPriceTry = toTryAmount(catalogMaterial.listPrice, catalogMaterial.currency || 'TRY');
  return listPriceTry * (1 - catalogMaterial.discount / 100);
}

function formatListPriceWithCurrency(catalogMaterial) {
  const currency = catalogMaterial.currency || 'TRY';
  const symbol = currency === 'USD' ? '$' : currency === 'EUR' ? '€' : '₺';
  return `${symbol}${Number(catalogMaterial.listPrice || 0).toLocaleString('tr-TR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
}

function toTryAmount(amount, currency) {
  const value = Number(amount || 0);
  if (currency === 'USD') return value * Number(getGlobalCatalog().exchangeRates.usd || 1);
  if (currency === 'EUR') return value * Number(getGlobalCatalog().exchangeRates.eur || 1);
  return value;
}

function calcMaterialUnitPriceForData(catalogMaterial, data) {
  const value = Number(catalogMaterial.listPrice || 0);
  const currency = catalogMaterial.currency || 'TRY';
  const listPriceTry = currency === 'USD'
    ? value * Number(getGlobalCatalog().exchangeRates.usd || 1)
    : currency === 'EUR'
      ? value * Number(getGlobalCatalog().exchangeRates.eur || 1)
      : value;
  return listPriceTry * (1 - catalogMaterial.discount / 100);
}

function getLineSnapshotSource(line, catalog) {
  if (line.snapshot) return line.snapshot;
  if (catalog) {
    return {
      listPrice: Number(catalog.listPrice || 0),
      currency: catalog.currency || 'TRY',
      discount: Number(catalog.discount || 0),
      laborUnitPrice: Number(catalog.laborUnitPrice || 0),
    };
  }
  return null;
}

function getLinePricing(line) {
  const catalog = getMaterialCatalog().find((m) => m.id === line.catalogMaterialId);
  const source = getLineSnapshotSource(line, catalog);
  if (!source) return null;

  const unitMaterial = calcMaterialUnitPrice(source);
  const laborUnitPrice = Number(source.laborUnitPrice || 0);
  return {
    catalog,
    source,
    unitMaterial,
    laborUnitPrice,
    materialTotal: unitMaterial * line.quantity,
    laborTotal: laborUnitPrice * line.quantity,
  };
}

function isLinePriceOutdated(line, catalog) {
  if (!catalog || !line.snapshot) return false;
  return (
    Number(line.snapshot.listPrice || 0) !== Number(catalog.listPrice || 0) ||
    String(line.snapshot.currency || 'TRY') !== String(catalog.currency || 'TRY') ||
    Number(line.snapshot.discount || 0) !== Number(catalog.discount || 0) ||
    Number(line.snapshot.laborUnitPrice || 0) !== Number(catalog.laborUnitPrice || 0)
  );
}

function updateLineSnapshotFromCatalog(line, catalog) {
  if (!catalog) return;
  line.snapshot = {
    listPrice: Number(catalog.listPrice || 0),
    currency: catalog.currency || 'TRY',
    discount: Number(catalog.discount || 0),
    laborUnitPrice: Number(catalog.laborUnitPrice || 0),
  };
}


function getDetailSums(detailId) {
  const lines = getData().lineItemsByDetail[detailId] || [];
  return lines.reduce(
    (acc, line) => {
      const pricing = getLinePricing(line);
      if (!pricing) return acc;
      acc.material += pricing.materialTotal;
      acc.labor += pricing.laborTotal;
      return acc;
    },
    { material: 0, labor: 0 },
  );
}

function getGroupSums(groupId) {
  return getData().jobDetails
    .filter((d) => d.groupId === groupId)
    .reduce(
      (acc, d) => {
        const sums = getDetailSums(d.id);
        acc.material += sums.material;
        acc.labor += sums.labor;
        return acc;
      },
      { material: 0, labor: 0 },
    );
}

function getDetailTotal(detailId) {
  const lines = getData().lineItemsByDetail[detailId] || [];
  return lines.reduce((sum, line) => {
    const pricing = getLinePricing(line);
    if (!pricing) return sum;
    return sum + pricing.materialTotal + pricing.laborTotal;
  }, 0);
}

function getGroupTotal(groupId) {
  return getData().jobDetails
    .filter((d) => d.groupId === groupId)
    .reduce((sum, d) => sum + getDetailTotal(d.id), 0);
}


function exportMainToExcel() {
  const headers = ['Tesisat Grubu', 'Tutar', 'Detay'];
  const rows = getData().plumbingGroups.map((g) => [
    g.name,
    Number(getGroupTotal(g.id)).toFixed(2),
    (g.description || '').replaceAll('\n', ' '),
  ]);
  const grandTotal = getData().plumbingGroups.reduce((sum, g) => sum + getGroupTotal(g.id), 0);
  rows.push(['GENEL TOPLAM', Number(grandTotal).toFixed(2), '']);

  const csv = [headers, ...rows]
    .map((row) => row.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(';'))
    .join('\n');

  const blob = new Blob([`\ufeff${csv}`], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = 'ana-form.csv';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

function showCatalogView() {
  showView('catalog');
  const global = getGlobalCatalog();

  const groupOptions = global.plumbingGroups
    .map((g) => `<option value="${g.id}">${g.name}</option>`)
    .join('');

  const groupRows = global.plumbingGroups
    .map(
      (g) => `<tr>
        <td>${g.name}</td>
        <td>
          <div class="actions">
            <button data-edit-global-group="${g.id}">Düzenle</button>
            <button class="danger" data-delete-global-group="${g.id}">Sil</button>
          </div>
        </td>
      </tr>`,
    )
    .join('');

  const detailRows = global.jobDetails
    .map((d) => {
      const group = global.plumbingGroups.find((g) => g.id === d.groupId);
      return `<tr>
        <td>${group?.name || '-'}</td>
        <td>${d.name}</td>
        <td>${d.description || ''}</td>
        <td>
          <div class="actions">
            <button data-edit-global-detail="${d.id}">Düzenle</button>
            <button class="danger" data-delete-global-detail="${d.id}">Sil</button>
          </div>
        </td>
      </tr>`;
    })
    .join('');

  views.catalog.innerHTML = `
    <div class="card">
      <h2>Tesisat Grubu ve İş Detayı Grubu Oluşturma</h2>
      <form id="globalGroupForm" class="grid">
        <label>Tesisat Grubu
          <input name="groupName" required placeholder="Örn. Sıhhi" />
        </label>
        <label><button class="primary" type="submit">Tesisat Grubu Ekle</button></label>
      </form>
      <form id="globalDetailForm" class="grid">
        <label>Tesisat Grubu
          <select name="groupId" required><option value="">Seçiniz</option>${groupOptions}</select>
        </label>
        <label>İş Detayı Grubu
          <input name="detailName" required placeholder="Örn. Vitrifiye" />
        </label>
        <label>Detay
          <input name="description" placeholder="Açıklama" />
        </label>
        <label><button class="primary" type="submit">İş Detayı Grubu Ekle</button></label>
      </form>
    </div>

    <div class="card">
      <h3>Tesisat Grupları</h3>
      <table>
        <thead><tr><th>Tesisat Grubu</th><th>İşlem</th></tr></thead>
        <tbody>${groupRows || '<tr><td colspan="2">Henüz tesisat grubu yok.</td></tr>'}</tbody>
      </table>
    </div>

    <div class="card">
      <h3>İş Detayı Grupları</h3>
      <table>
        <thead><tr><th>Tesisat Grubu</th><th>İş Detayı Grubu</th><th>Detay</th><th>İşlem</th></tr></thead>
        <tbody>${detailRows || '<tr><td colspan="4">Henüz iş detayı grubu yok.</td></tr>'}</tbody>
      </table>
    </div>
  `;

  document.getElementById('globalGroupForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const name = String(fd.get('groupName')).trim();
    if (!name) return;
    global.plumbingGroups.push({ id: uid('grp'), name, description: '' });
    saveState();
    showCatalogView();
  });

  document.getElementById('globalDetailForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const groupId = String(fd.get('groupId'));
    const detailName = String(fd.get('detailName')).trim();
    if (!groupId || !detailName) return;
    global.jobDetails.push({
      id: uid('det'),
      groupId,
      name: detailName,
      description: String(fd.get('description') || '').trim(),
    });
    saveState();
    showCatalogView();
  });

  views.catalog.querySelectorAll('[data-edit-global-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const group = global.plumbingGroups.find((g) => g.id === btn.dataset.editGlobalGroup);
      if (!group) return;
      const name = prompt('Tesisat grubu adı', group.name);
      if (name === null) return;
      group.name = name.trim() || group.name;

      state.proposals.forEach((proposal) => {
        proposal.data.plumbingGroups.forEach((g) => {
          if (g.id === group.id) g.name = group.name;
        });
      });

      saveState();
      showCatalogView();
    });
  });

  views.catalog.querySelectorAll('[data-delete-global-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const groupId = btn.dataset.deleteGlobalGroup;
      const detailIds = global.jobDetails.filter((d) => d.groupId === groupId).map((d) => d.id);

      global.plumbingGroups = global.plumbingGroups.filter((g) => g.id !== groupId);
      global.jobDetails = global.jobDetails.filter((d) => d.groupId !== groupId);
      global.materialCatalog = global.materialCatalog.filter(
        (m) => m.groupId !== groupId && !detailIds.includes(m.jobDetailId),
      );

      state.proposals.forEach((proposal) => {
        proposal.data.plumbingGroups = proposal.data.plumbingGroups.filter((g) => g.id !== groupId);
        proposal.data.jobDetails = proposal.data.jobDetails.filter((d) => d.groupId !== groupId);
        detailIds.forEach((id) => delete proposal.data.lineItemsByDetail[id]);
      });

      saveState();
      showCatalogView();
    });
  });

  views.catalog.querySelectorAll('[data-edit-global-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const detail = global.jobDetails.find((d) => d.id === btn.dataset.editGlobalDetail);
      if (!detail) return;

      const selectedGroup = prompt('Tesisat Grubu ID', detail.groupId);
      if (selectedGroup === null) return;
      const name = prompt('İş detayı grubu adı', detail.name);
      if (name === null) return;
      const desc = prompt('Detay', detail.description || '');
      if (desc === null) return;

      const groupId = selectedGroup.trim();
      const groupExists = global.plumbingGroups.some((g) => g.id === groupId);
      if (!groupExists) {
        alert('Geçerli bir tesisat grubu seçiniz.');
        return;
      }

      detail.groupId = groupId;
      detail.name = name.trim() || detail.name;
      detail.description = desc.trim();

      state.proposals.forEach((proposal) => {
        proposal.data.jobDetails.forEach((d) => {
          if (d.id === detail.id) {
            d.groupId = detail.groupId;
            d.name = detail.name;
            d.description = detail.description;
          }
        });
      });

      saveState();
      showCatalogView();
    });
  });

  views.catalog.querySelectorAll('[data-delete-global-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const detailId = btn.dataset.deleteGlobalDetail;

      global.jobDetails = global.jobDetails.filter((d) => d.id !== detailId);
      global.materialCatalog = global.materialCatalog.filter((m) => m.jobDetailId !== detailId);

      state.proposals.forEach((proposal) => {
        proposal.data.jobDetails = proposal.data.jobDetails.filter((d) => d.id !== detailId);
        delete proposal.data.lineItemsByDetail[detailId];
      });

      saveState();
      showCatalogView();
    });
  });
}

function showMainView() {
  const activeProposal = getActiveProposal();
  if (!activeProposal) return showQuotesView();

  showView('main');

  const rows = getData().plumbingGroups
    .map(
      (g) => `
      <tr>
        <td>${g.name}</td>
        <td class="right">${formatMoney(getGroupTotal(g.id))}</td>
        <td>
          <div class="actions">
            <button data-open-group="${g.id}">Aç</button>
            
            <button class="danger" data-delete-group="${g.id}">Sil</button>
          </div>
        </td>
      </tr>`,
    )
    .join('');

  views.main.innerHTML = `
    <div class="card">
      <h2>Teklif Ana Kalemleri</h2>
      <p class="small"><b>Firma:</b> ${activeProposal.firmName} | <b>Proje:</b> ${activeProposal.projectName}</p>
      <p class="small">Sadece Tesisat Grubu, Tutarı ve Detayı görüntülenir.</p>
      <form id="groupForm" class="grid">
        <label>Tesisat Grubu
          <select name="groupId" required>
            <option value="">Seçiniz</option>
            ${getGlobalCatalog().plumbingGroups.map((g)=>`<option value="${g.id}">${g.name}</option>`).join('')}
          </select>
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Tesisat Grubunu Teklife Ekle</button>
        </label>
        <label style="align-self:end;">
          <button id="exportMainExcel" type="button">Ana Sayfayı Excele Aktar</button>
        </label>
      </form>
      <datalist id="materialNameSuggestions"></datalist>
      <datalist id="materialBrandSuggestions"></datalist>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr>
            <th>Tesisat Grubu</th>
            <th class="right">Tutar</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          ${rows || '<tr><td colspan="3">Henüz tesisat grubu yok.</td></tr>'}
        </tbody>
        <tfoot>
          <tr>
            <td><b>Genel Toplam</b></td>
            <td class="right total">${formatMoney(getData().plumbingGroups.reduce((sum, g) => sum + getGroupTotal(g.id), 0))}</td>
            <td></td>
          </tr>
          <tr>
            <td><b>KDV Tutarı (%20)</b></td>
            <td class="right total">${formatMoney(getData().plumbingGroups.reduce((sum, g) => sum + getGroupTotal(g.id), 0) * 0.2)}</td>
            <td></td>
          </tr>
          <tr>
            <td><b>KDV Dahil Toplam Tutar</b></td>
            <td class="right total">${formatMoney(getData().plumbingGroups.reduce((sum, g) => sum + getGroupTotal(g.id), 0) * 1.2)}</td>
            <td></td>
          </tr>
        </tfoot>
      </table>
    </div>
  `;

  const groupForm = document.getElementById('groupForm');
  document.getElementById('exportMainExcel').addEventListener('click', exportMainToExcel);

  groupForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const formData = new FormData(groupForm);
    const group = getGlobalCatalog().plumbingGroups.find((g) => g.id === String(formData.get('groupId')));
    if (!group) return;
    const exists = getData().plumbingGroups.some((g) => g.id === group.id);
    if (exists) return;
    getData().plumbingGroups.push({ ...group });
    saveState();
    showMainView();
  });

  views.main.querySelectorAll('[data-open-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      currentGroupId = btn.dataset.openGroup;
      showGroupView();
    });
  });


  views.main.querySelectorAll('[data-delete-group]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const groupId = btn.dataset.deleteGroup;
      const detailIds = getData().jobDetails.filter((d) => d.groupId === groupId).map((d) => d.id);
      getData().plumbingGroups = getData().plumbingGroups.filter((g) => g.id !== groupId);
      getData().jobDetails = getData().jobDetails.filter((d) => d.groupId !== groupId);
      detailIds.forEach((id) => delete getData().lineItemsByDetail[id]);
      saveState();
      showMainView();
    });
  });
}

function showGroupView() {
  const group = getGlobalCatalog().plumbingGroups.find((g) => g.id === currentGroupId);
  if (!group) return showMainView();

  showView('group');
  const details = getData().jobDetails.filter((d) => d.groupId === group.id);
  const groupSums = getGroupSums(group.id);

  const rows = details
    .map(
      (d) => `
      <tr>
        <td>${d.name}</td>
        <td class="right">${formatMoney(getDetailTotal(d.id))}</td>
        <td>
          <div class="actions">
            <button data-open-detail="${d.id}">İş Detayına Gir</button>
            
            <button class="danger" data-delete-detail="${d.id}">Sil</button>
          </div>
        </td>
      </tr>`,
    )
    .join('');

  views.group.innerHTML = `
    <div class="card">
      <div class="actions"><button id="backToMain">← Teklif Ana Kalemlerine Dön</button></div>
      <h2>${group.name} Tesisat Grubu</h2>
      <p class="small">İş Detayı, toplam tutar ve detay görüntülenir.</p>
      <form id="detailForm" class="grid">
        <label>İş Detayı Grubu
          <select name="detailId" required>
            <option value="">Seçiniz</option>
            ${getGlobalCatalog().jobDetails.filter((d)=>d.groupId===group.id).map((d)=>`<option value="${d.id}">${d.name}</option>`).join('')}
          </select>
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">İş Detayı Grubunu Teklife Ekle</button>
        </label>
      </form>
      <datalist id="materialNameSuggestions"></datalist>
      <datalist id="materialBrandSuggestions"></datalist>
    </div>

    <div class="card">
      <table>
        <thead>
          <tr><th>İş Detayı</th><th class="right">Toplam Tutar</th><th>İşlem</th></tr>
        </thead>
        <tbody>
          ${rows || '<tr><td colspan="3">Bu grupta iş detayı yok.</td></tr>'}
        </tbody>
        <tfoot>
          <tr><td><b>İş detaylarının içindeki Malzemelerin Toplamı</b></td><td class="right total">${formatMoney(groupSums.material)}</td><td></td></tr>
          <tr><td><b>İş detaylarının içindeki İşçilikler Toplamı</b></td><td class="right total">${formatMoney(groupSums.labor)}</td><td></td></tr>
          <tr><td><b>Genel Toplam</b></td><td class="right total">${formatMoney(groupSums.material + groupSums.labor)}</td><td></td></tr>
        </tfoot>
      </table>
    </div>
  `;

  document.getElementById('backToMain').addEventListener('click', showMainView);
  const detailForm = document.getElementById('detailForm');
  detailForm.addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(detailForm);
    const globalDetail = getGlobalCatalog().jobDetails.find((d) => d.id === String(fd.get('detailId')));
    if (!globalDetail) return;
    const exists = getData().jobDetails.some((d) => d.id === globalDetail.id);
    if (exists) return;
    getData().jobDetails.push({ ...globalDetail });
    saveState();
    showGroupView();
  });

  views.group.querySelectorAll('[data-open-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      currentDetailId = btn.dataset.openDetail;
      showDetailView();
    });
  });


  views.group.querySelectorAll('[data-delete-detail]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      const id = btn.dataset.deleteDetail;
      getData().jobDetails = getData().jobDetails.filter((d) => d.id !== id);
      delete getData().lineItemsByDetail[id];
      saveState();
      showGroupView();
    });
  });
}

function showDetailView() {
  const detail = getData().jobDetails.find((d) => d.id === currentDetailId);
  if (!detail) return showGroupView();

  const group = getData().plumbingGroups.find((g) => g.id === detail.groupId);
  const catalogForDetail = getMaterialCatalog().filter((m) => m.jobDetailId === detail.id);
  const materialNames = [...new Set(catalogForDetail.map((m) => m.name))];
  const lines = getData().lineItemsByDetail[detail.id] || [];

  showView('detail');

  const detailSums = lines.reduce(
    (acc, line) => {
      const pricing = getLinePricing(line);
      if (!pricing) return acc;
      acc.material += pricing.materialTotal;
      acc.labor += pricing.laborTotal;
      return acc;
    },
    { material: 0, labor: 0 },
  );

  const rowHtml = lines
    .map((line) => {
      const pricing = getLinePricing(line);
      if (!pricing) return '';
      const catalog = pricing.catalog || {};
      const source = pricing.source;
      const outdated = isLinePriceOutdated(line, pricing.catalog);
      return `
      <tr>
        <td>${catalog.name || '-'}</td>
        <td>${catalog.brand || '-'}</td>
        <td class="right">${line.quantity}</td>
        <td class="right">${formatListPriceWithCurrency(source)}</td>
        <td class="right">%${source.discount}</td>
        <td class="right">${formatMoney(pricing.unitMaterial)}</td>
        <td class="right">${formatMoney(pricing.laborUnitPrice)}</td>
        <td class="right">${formatMoney(pricing.materialTotal)}</td>
        <td class="right">${formatMoney(pricing.laborTotal)}</td>
        <td class="right">${formatMoney(pricing.materialTotal + pricing.laborTotal)}</td>
        <td>
          <div class="actions">
            ${outdated ? '<span style="color:#c62828;font-weight:bold;">●</span><button data-refresh-line="'+line.id+'">Fiyatı Güncelle</button>' : ''}
            <button class="danger" data-delete-line="${line.id}">Sil</button>
          </div>
        </td>
      </tr>`;
    })
    .join('');

  views.detail.innerHTML = `
    <div class="card">
      <div class="actions"><button id="backToGroup">← ${group.name} grubuna dön</button></div>
      <h2>${detail.name} İş Detayı</h2>
      <p class="small">Malzeme, marka, adet, liste fiyat, iskonto ve işçilikten toplam tutar otomatik hesaplanır.</p>
      <form id="lineForm" class="grid">
        <label>Malzeme
          <select id="lineMaterialName" name="materialName" required>
            <option value="">Seçiniz</option>
            ${materialNames.map((name) => `<option value="${name}">${name}</option>`).join('')}
          </select>
        </label>
        <label>Marka
          <select id="lineBrand" name="brand" required>
            <option value="">Önce malzeme seçin</option>
          </select>
        </label>
        <label>Adet
          <input type="number" step="1" min="1" name="quantity" required value="1" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Satır Ekle</button>
        </label>
      </form>
      <datalist id="materialNameSuggestions"></datalist>
      <datalist id="materialBrandSuggestions"></datalist>
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
          <tr><td colspan="10" class="right"><b>Malzemelerin Toplam Tutarı</b></td><td class="right total">${formatMoney(detailSums.material)}</td><td></td></tr>
          <tr><td colspan="10" class="right"><b>İşçilik Toplamı</b></td><td class="right total">${formatMoney(detailSums.labor)}</td><td></td></tr>
          <tr><td colspan="10" class="right"><b>Genel Toplam</b></td><td class="right total">${formatMoney(detailSums.material + detailSums.labor)}</td><td></td></tr>
        </tfoot>
      </table>
    </div>
  `;

  document.getElementById('backToGroup').addEventListener('click', showGroupView);

  const materialNameSelect = document.getElementById('lineMaterialName');
  const brandSelect = document.getElementById('lineBrand');

  const fillBrands = () => {
    const selectedName = materialNameSelect.value;
    const brands = catalogForDetail.filter((m) => m.name === selectedName);
    brandSelect.innerHTML = `
      <option value="">Seçiniz</option>
      ${brands
        .map((m) => `<option value="${m.id}">${m.brand} (${formatMoney(calcMaterialUnitPrice(m))})</option>`)
        .join('')}
    `;
  };

  materialNameSelect.addEventListener('change', fillBrands);

  document.getElementById('lineForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const selectedCatalogId = String(fd.get('brand'));
    if (!selectedCatalogId) {
      alert('Lütfen marka seçiniz.');
      return;
    }

    const selectedCatalog = getMaterialCatalog().find((m) => m.id === selectedCatalogId);
    if (!selectedCatalog) return;

    const line = {
      id: uid('line'),
      catalogMaterialId: selectedCatalogId,
      quantity: Number(fd.get('quantity')),
      snapshot: {
        listPrice: Number(selectedCatalog.listPrice || 0),
        currency: selectedCatalog.currency || 'TRY',
        discount: Number(selectedCatalog.discount || 0),
        laborUnitPrice: Number(selectedCatalog.laborUnitPrice || 0),
      },
    };
    if (!getData().lineItemsByDetail[detail.id]) getData().lineItemsByDetail[detail.id] = [];
    getData().lineItemsByDetail[detail.id].push(line);
    saveState();
    showDetailView();
  });


  views.detail.querySelectorAll('[data-refresh-line]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const line = (getData().lineItemsByDetail[detail.id] || []).find((l) => l.id === btn.dataset.refreshLine);
      if (!line) return;
      const catalog = getMaterialCatalog().find((m) => m.id === line.catalogMaterialId);
      if (!catalog) return;
      updateLineSnapshotFromCatalog(line, catalog);
      saveState();
      showDetailView();
    });
  });

  views.detail.querySelectorAll('[data-delete-line]').forEach((btn) => {
    btn.addEventListener('click', () => {
      if (!confirm('Silmek istiyor musunuz?')) return;
      getData().lineItemsByDetail[detail.id] = (getData().lineItemsByDetail[detail.id] || []).filter(
        (l) => l.id !== btn.dataset.deleteLine,
      );
      saveState();
      showDetailView();
    });
  });
}

function showMaterialsView(options = {}) {
  showView('materials');

  const groupOptions = getGlobalCatalog().plumbingGroups
    .map((g) => `<option value="${g.id}">${g.name}</option>`)
    .join('');

  views.materials.innerHTML = `
    <div class="card">
      <h2>Malzeme Yönetimi</h2>
      <p class="small">Tesisat Grubu ve İş Detayı seçip malzeme, marka, fiyat ve iskonto ile kayıt açın.</p>
      <div class="grid">
        <label>Dolar Kuru (TL)
          <input id="usdRateInput" type="number" step="0.0001" min="0" value="${getGlobalCatalog().exchangeRates.usd}" />
        </label>
        <label>Euro Kuru (TL)
          <input id="eurRateInput" type="number" step="0.0001" min="0" value="${getGlobalCatalog().exchangeRates.eur}" />
        </label>
      </div>
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
          <input id="materialNameInput" name="name" required placeholder="Örn. Lavabo" list="materialNameSuggestions" />
        </label>
        <label>Marka
          <input id="materialBrandInput" name="brand" required placeholder="Örn. ECA" list="materialBrandSuggestions" />
        </label>
        <label>Liste Fiyatı
          <input id="materialListPriceInput" type="number" step="0.01" min="0" name="listPrice" required />
        </label>
        <label>Para Birimi
          <select id="materialCurrencySelect" name="currency" required>
            <option value="TRY">TL</option>
            <option value="USD">Dolar</option>
            <option value="EUR">Euro</option>
          </select>
        </label>
        <label>İskonto (%)
          <input id="materialDiscountInput" type="number" step="0.01" min="0" max="100" name="discount" required value="0" />
        </label>
        <label>İşçilik Fiyatı
          <input id="materialLaborInput" type="number" step="0.01" min="0" name="laborUnitPrice" required value="0" />
        </label>
        <label style="align-self:end;">
          <button class="primary" type="submit">Ekle</button>
        </label>
      </form>
      <datalist id="materialNameSuggestions"></datalist>
      <datalist id="materialBrandSuggestions"></datalist>
    </div>

    <div class="card">
      <h3>Kayıtlı Malzemeler</h3>
      <table>
        <thead>
          <tr>
            <th>Tesisat Grubu</th><th>İş Detayı</th><th>Malzeme</th><th>Marka</th><th class="right">Liste Fiyatı</th><th>Para Birimi</th><th class="right">İskonto</th><th class="right">İşçilik Fiyatı</th><th class="right">Birim Fiyat (TL)</th><th>İşlem</th>
          </tr>
        </thead>
        <tbody id="materialsTableBody"></tbody>
      </table>
    </div>
  `;

  const groupSelect = document.getElementById('materialGroupSelect');
  const detailSelect = document.getElementById('materialDetailSelect');
  const materialNameInput = document.getElementById('materialNameInput');
  const materialBrandInput = document.getElementById('materialBrandInput');
  const materialListPriceInput = document.getElementById('materialListPriceInput');
  const materialCurrencySelect = document.getElementById('materialCurrencySelect');
  const usdRateInput = document.getElementById('usdRateInput');
  const eurRateInput = document.getElementById('eurRateInput');
  const materialDiscountInput = document.getElementById('materialDiscountInput');
  const materialLaborInput = document.getElementById('materialLaborInput');
  const materialNameSuggestions = document.getElementById('materialNameSuggestions');
  const materialBrandSuggestions = document.getElementById('materialBrandSuggestions');
  const tableBody = document.getElementById('materialsTableBody');

  const saveRates = () => {
    getGlobalCatalog().exchangeRates.usd = Number(usdRateInput.value || 1);
    getGlobalCatalog().exchangeRates.eur = Number(eurRateInput.value || 1);
    saveState();
    renderAutoCompleteLists();
    renderMaterialsTable();
  };

  usdRateInput.addEventListener('change', saveRates);
  eurRateInput.addEventListener('change', saveRates);

  const fillDetails = () => {
    const details = getGlobalCatalog().jobDetails.filter((d) => d.groupId === groupSelect.value);
    const previousDetailValue = detailSelect.value;
    detailSelect.innerHTML = `
      <option value="">Seçiniz</option>
      ${details.map((d) => `<option value="${d.id}">${d.name}</option>`).join('')}
    `;

    const hasPrevious = details.some((d) => d.id === previousDetailValue);
    const desiredValue = options.selectedDetailId || previousDetailValue;
    if (desiredValue && details.some((d) => d.id === desiredValue)) {
      detailSelect.value = desiredValue;
    } else if (hasPrevious) {
      detailSelect.value = previousDetailValue;
    }
  };

  const renderAutoCompleteLists = () => {
    const selectedGroupId = groupSelect.value;
    const selectedDetailId = detailSelect.value;
    const typedName = materialNameInput.value.trim().toLowerCase();
    const typedBrand = materialBrandInput.value.trim().toLowerCase();

    const scopedMaterials = getMaterialCatalog().filter((m) => {
      const groupMatch = !selectedGroupId || m.groupId === selectedGroupId;
      const detailMatch = !selectedDetailId || m.jobDetailId === selectedDetailId;
      return groupMatch && detailMatch;
    });

    const nameOptions = [...new Set(scopedMaterials.map((m) => m.name))]
      .filter((name) => !typedName || name.toLowerCase().includes(typedName));

    const brandPool = scopedMaterials.filter(
      (m) => !typedName || m.name.toLowerCase().includes(typedName),
    );
    const brandOptions = [...new Set(brandPool.map((m) => m.brand))]
      .filter((brand) => !typedBrand || brand.toLowerCase().includes(typedBrand));

    materialNameSuggestions.innerHTML = nameOptions
      .map((name) => `<option value="${name}"></option>`)
      .join('');

    materialBrandSuggestions.innerHTML = brandOptions
      .map((brand) => `<option value="${brand}"></option>`)
      .join('');
  };

  const getFilteredMaterials = () => {
    const selectedGroupId = groupSelect.value;
    const selectedDetailId = detailSelect.value;
    const nameQuery = materialNameInput.value.trim().toLowerCase();
    const brandQuery = materialBrandInput.value.trim().toLowerCase();

    return getMaterialCatalog().filter((m) => {
      const groupMatch = !selectedGroupId || m.groupId === selectedGroupId;
      const detailMatch = !selectedDetailId || m.jobDetailId === selectedDetailId;
      const nameMatch = !nameQuery || m.name.toLowerCase().includes(nameQuery);
      const brandMatch = !brandQuery || m.brand.toLowerCase().includes(brandQuery);
      return groupMatch && detailMatch && nameMatch && brandMatch;
    });
  };

  const renderMaterialsTable = () => {
    const filtered = getFilteredMaterials();

    const rows = filtered
      .map((m) => {
        const grp = getGlobalCatalog().plumbingGroups.find((g) => g.id === m.groupId);
        const det = getGlobalCatalog().jobDetails.find((d) => d.id === m.jobDetailId);
        return `<tr>
          <td>${grp?.name || '-'}</td>
          <td>${det?.name || '-'}</td>
          <td>${m.name}</td>
          <td>${m.brand}</td>
          <td class="right">${formatListPriceWithCurrency(m)}</td>
          <td>${m.currency || 'TRY'}</td>
          <td class="right">%${m.discount}</td>
          <td class="right">${formatMoney(m.laborUnitPrice || 0)}</td>
          <td class="right">${formatMoney(calcMaterialUnitPrice(m))}</td>
          <td>
            <div class="actions">
              <button data-edit-material="${m.id}">Düzenle</button>
              <button class="danger" data-delete-material="${m.id}">Sil</button>
            </div>
          </td>
        </tr>`;
      })
      .join('');

    tableBody.innerHTML = rows || '<tr><td colspan="10">Kayıt yok.</td></tr>';

    views.materials.querySelectorAll('[data-delete-material]').forEach((btn) => {
      btn.addEventListener('click', () => {
        if (!confirm('Silmek istiyor musunuz?')) return;
        const materialId = btn.dataset.deleteMaterial;
        getMaterialCatalog() = getMaterialCatalog().filter((m) => m.id !== materialId);
        Object.keys(getData().lineItemsByDetail).forEach((detailId) => {
          getData().lineItemsByDetail[detailId] = getData().lineItemsByDetail[detailId].filter(
            (line) => line.catalogMaterialId !== materialId,
          );
        });
        saveState();
        renderAutoCompleteLists();
        renderMaterialsTable();
      });
    });

    views.materials.querySelectorAll('[data-edit-material]').forEach((btn) => {
      btn.addEventListener('click', () => {
        const material = getMaterialCatalog().find((m) => m.id === btn.dataset.editMaterial);
        if (!material) return;

        const name = prompt('Malzeme adı', material.name);
        if (name === null) return;
        const brand = prompt('Marka', material.brand);
        if (brand === null) return;
        const listPrice = prompt('Liste fiyatı', String(material.listPrice));
        if (listPrice === null) return;
        const discount = prompt('İskonto (%)', String(material.discount));
        if (discount === null) return;
        const currency = prompt('Para birimi (TRY/USD/EUR)', String(material.currency || 'TRY'));
        if (currency === null) return;
        const laborUnitPrice = prompt('İşçilik fiyatı', String(material.laborUnitPrice || 0));
        if (laborUnitPrice === null) return;

        const normalizedName = name.trim();
        const normalizedBrand = brand.trim();

        const duplicate = getMaterialCatalog().some(
          (m) =>
            m.id !== material.id &&
            m.name.toLowerCase() === normalizedName.toLowerCase() &&
            m.brand.toLowerCase() === normalizedBrand.toLowerCase(),
        );

        if (duplicate) {
          alert('Aynı malzeme adı ve marka ile ikinci bir kayıt eklenemez.');
          return;
        }

        material.name = normalizedName || material.name;
        material.brand = normalizedBrand || material.brand;
        material.listPrice = Number(listPrice);
        const normalizedCurrency = String(currency).trim().toUpperCase();
        if (!['TRY', 'USD', 'EUR'].includes(normalizedCurrency)) {
          alert('Para birimi TRY, USD veya EUR olmalıdır.');
          return;
        }

        material.discount = Number(discount);
        material.currency = normalizedCurrency;
        material.laborUnitPrice = Number(laborUnitPrice);

        saveState();
        renderAutoCompleteLists();
        renderMaterialsTable();
      });
    });
  };

  groupSelect.addEventListener('change', () => {
    fillDetails();
    renderAutoCompleteLists();
    renderMaterialsTable();
  });
  detailSelect.addEventListener('change', () => {
    renderAutoCompleteLists();
    renderMaterialsTable();
  });
  materialNameInput.addEventListener('input', () => {
    renderAutoCompleteLists();
    renderMaterialsTable();
  });
  materialBrandInput.addEventListener('input', () => {
    renderAutoCompleteLists();
    renderMaterialsTable();
  });

  document.getElementById('materialForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    const groupId = String(fd.get('groupId'));
    const jobDetailId = String(fd.get('jobDetailId'));
    const name = String(fd.get('name')).trim();
    const brand = String(fd.get('brand')).trim();
    const currency = String(fd.get('currency') || 'TRY').toUpperCase();

    const duplicate = getMaterialCatalog().some(
      (m) => m.name.toLowerCase() === name.toLowerCase() && m.brand.toLowerCase() === brand.toLowerCase(),
    );

    if (duplicate) {
      alert('Aynı malzeme adı ve marka ile ikinci bir kayıt eklenemez.');
      return;
    }

    getMaterialCatalog().push({
      id: uid('mat'),
      groupId,
      jobDetailId,
      name,
      brand,
      listPrice: Number(fd.get('listPrice')),
      currency,
      discount: Number(fd.get('discount')),
      laborUnitPrice: Number(fd.get('laborUnitPrice')),
    });

    saveState();
    showMaterialsView({
      selectedGroupId: groupId,
      selectedDetailId: jobDetailId,
      name,
    });
  });

  if (options.selectedGroupId) {
    groupSelect.value = options.selectedGroupId;
  }
  fillDetails();

  materialNameInput.value = options.name || '';
  materialBrandInput.value = options.brand || '';
  materialListPriceInput.value = options.listPrice || '';
  materialCurrencySelect.value = options.currency || 'TRY';
  materialDiscountInput.value = options.discount || '0';
  materialLaborInput.value = options.laborUnitPrice || '0';

  renderAutoCompleteLists();
  renderMaterialsTable();
}
