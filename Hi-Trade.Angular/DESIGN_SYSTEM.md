# Hi-Trade Design System & Page Authoring Guide

> **Style Direction**: *Nordic Obsidian & Cyber-Emerald Institutional FinTech*  
> High-contrast precision dark mode, tabular monospace numerals, hairline frosted borders, and semantic financial action states.

---

## 1. Quick Start: Creating a New Page

To create a new page, use the standardized container and header classes in your component template without needing custom CSS:

```html
<app-nav-bar-component></app-nav-bar-component>

<main class="ht-page-container">
  <!-- Page Header -->
  <header class="ht-page-header">
    <div>
      <div class="flex items-center gap-2 mb-1">
        <span class="ht-badge ht-badge-neutral">SECTION BADGE</span>
        <span class="ht-badge ht-badge-success">
          <span class="ht-status-dot"></span> LIVE
        </span>
      </div>
      <h1 class="ht-page-title">Page Title</h1>
      <p class="ht-page-subtitle">Descriptive summary of the functionality on this view.</p>
    </div>

    <!-- Actions Toolbar -->
    <div class="ht-page-actions">
      <button type="button" class="ht-btn ht-btn-secondary">
        Export Data
      </button>
      <button type="button" class="ht-btn ht-btn-primary">
        + Primary Action
      </button>
    </div>
  </header>

  <!-- Content (Stat Grid, Tables, Cards) -->
  ...
</main>
```

---

## 2. Design Tokens (`:root` in `src/styles.css`)

| Token | Value | Purpose |
| :--- | :--- | :--- |
| `--ht-bg-app` | `#080c14` | Primary app canvas background |
| `--ht-bg-surface` | `#0e1526` | Card and modal body surface |
| `--ht-bg-surface-elevated` | `#141f36` | Elevated cards, secondary buttons |
| `--ht-bg-card` | `#0b1120` | Standard card background |
| `--ht-bg-input` | `#060a14` | Input, select, and textarea fields |
| `--ht-border-subtle` | `rgba(255, 255, 255, 0.08)` | Standard dividers, card outlines |
| `--ht-border-medium` | `rgba(255, 255, 255, 0.14)` | Inputs, active pills |
| `--ht-brand-primary` | `#10b981` | Brand emerald mint, primary CTA |
| `--ht-brand-hover` | `#059669` | Primary CTA hover state |
| `--ht-gain` / `--ht-gain-bg` | `#10b981` / `rgba(16, 185, 129, 0.12)` | Positive P&L, Buy actions |
| `--ht-loss` / `--ht-loss-bg` | `#f43f5e` / `rgba(244, 63, 94, 0.12)` | Negative P&L, Sell actions |
| `--ht-warning` / `--ht-warning-bg` | `#f59e0b` / `rgba(245, 158, 11, 0.12)` | Unverified email, warnings |
| `--ht-font-sans` | `Inter, -apple-system, sans-serif` | Clean UI typography |
| `--ht-font-mono` | `JetBrains Mono, monospace` | Financial numbers, prices, tickers |

---

## 3. Component Recipes

### 3.1 Stat / KPI Cards (`.ht-stat-grid` & `.ht-stat-card`)

```html
<div class="ht-stat-grid">
  <!-- Emerald Accent -->
  <div class="ht-stat-card ht-stat-card-accent-emerald">
    <div class="ht-stat-header">
      <span class="ht-stat-label">Total Portfolio Value</span>
      <span class="ht-badge ht-badge-neutral">Active</span>
    </div>
    <span class="ht-stat-val ht-tabular-num">$128,450.00</span>
    <span class="ht-stat-sub">Real-time mark-to-market</span>
  </div>

  <!-- Gain / Loss -->
  <div class="ht-stat-card ht-stat-card-accent-cyan">
    <div class="ht-stat-header">
      <span class="ht-stat-label">Realized Gain/Loss</span>
      <span class="ht-badge ht-badge-success">+14.2%</span>
    </div>
    <span class="ht-stat-val ht-tabular-num text-gain">+$12,340.50</span>
    <span class="ht-stat-sub">Cumulative trades</span>
  </div>
</div>
```

### 3.2 Standard & Elevated Cards (`.ht-card`)

```html
<div class="ht-card mb-6">
  <div class="ht-card-header">
    <h3 class="ht-card-title">Holdings Breakdown</h3>
    <span class="ht-badge ht-badge-neutral">8 Assets</span>
  </div>
  <div class="ht-card-body">
    <!-- Body content -->
  </div>
  <div class="ht-card-footer flex justify-between items-center">
    <span class="text-xs text-slate-400">Last updated: Just now</span>
    <button class="ht-btn ht-btn-sm ht-btn-secondary">Refresh</button>
  </div>
</div>
```

### 3.3 Data Tables (`.ht-table-container` & `.ht-table`)

Always use `.ht-tabular-num` for quantities, prices, currency, and percentages:

```html
<div class="ht-table-container">
  <table class="ht-table">
    <thead>
      <tr>
        <th>Ticker</th>
        <th>Instrument</th>
        <th class="text-right">Shares</th>
        <th class="text-right">Avg Price</th>
        <th class="text-right">Current Price</th>
        <th class="text-right">Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><span class="ht-badge ht-badge-ticker">NVDA</span></td>
        <td class="font-medium text-white">NVIDIA Corp.</td>
        <td class="text-right ht-tabular-num font-semibold">120</td>
        <td class="text-right ht-tabular-num text-slate-300">$640.00</td>
        <td class="text-right ht-tabular-num text-emerald-400 font-bold">$875.20</td>
        <td class="text-right">
          <button type="button" class="ht-btn ht-btn-danger ht-btn-sm">Sell</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

### 3.4 Buttons

```html
<!-- Primary (Emerald Gradient with Glow) -->
<button class="ht-btn ht-btn-primary">Execute Order</button>

<!-- Secondary (Dark Surface with Subtle Border) -->
<button class="ht-btn ht-btn-secondary">View History</button>

<!-- Buy / Positive -->
<button class="ht-btn ht-btn-success">+ Buy Asset</button>

<!-- Sell / Danger -->
<button class="ht-btn ht-btn-danger">Liquidate</button>

<!-- Ghost / Outline -->
<button class="ht-btn ht-btn-ghost">Dismiss</button>
<button class="ht-btn ht-btn-outline">Download CSV</button>

<!-- Sizes -->
<button class="ht-btn ht-btn-primary ht-btn-sm">Compact</button>
<button class="ht-btn ht-btn-primary ht-btn-lg">Large Hero</button>
```

### 3.5 Form Controls (`.ht-form-group`)

```html
<div class="ht-form-group">
  <label for="ticker" class="ht-label">Security Ticker</label>
  <input 
    id="ticker" 
    type="text" 
    class="ht-input ht-tabular-num uppercase" 
    placeholder="e.g. MSFT" 
  />
  <span class="ht-form-hint">Unique exchange-listed symbol.</span>
</div>

<!-- With Error -->
<div class="ht-form-group">
  <label for="shares" class="ht-label">Quantity</label>
  <input id="shares" type="number" class="ht-input" />
  <span class="ht-form-error">Quantity must be at least 1.</span>
</div>
```

### 3.6 Modals (`.ht-modal-overlay` & `.ht-modal-dialog`)

```html
@if (isOpen()) {
<div class="ht-modal-overlay" (click)="closeModal()">
  <div class="ht-modal-dialog" (click)="$event.stopPropagation()">
    <div class="ht-modal-header">
      <h2 class="ht-modal-title">Modal Header</h2>
      <button type="button" class="ht-modal-close" (click)="closeModal()">×</button>
    </div>

    <div class="ht-modal-body">
      <!-- Modal body content -->
    </div>

    <div class="ht-modal-footer">
      <button type="button" class="ht-btn ht-btn-secondary" (click)="closeModal()">Cancel</button>
      <button type="button" class="ht-btn ht-btn-primary" (click)="save()">Confirm Action</button>
    </div>
  </div>
</div>
}
```

### 3.7 Badges & Status Dots

```html
<span class="ht-badge ht-badge-success">
  <span class="ht-status-dot"></span> LIVE
</span>
<span class="ht-badge ht-badge-danger">CLOSED</span>
<span class="ht-badge ht-badge-warning">VERIFICATION REQUIRED</span>
<span class="ht-badge ht-badge-info">PRO TIER</span>
<span class="ht-badge ht-badge-ticker">AAPL</span>
<span class="ht-badge ht-badge-neutral">STANDARD</span>
```

### 3.8 Alerts & Banners

```html
<div class="ht-alert ht-alert-warning">
  <span>Your trading account has unverified email status.</span>
  <button class="ht-btn ht-btn-sm ht-btn-outline">Verify</button>
</div>
```

---

## 4. Key Developer Guardrails

1. **Avoid Inlining Heavy CSS**: Avoid adding multi-kilobyte styling in `*.component.css` files. Angular enforces an `anyComponentStyle` budget (4kB warning, 8kB error). Component `.css` files should only contain 10-30 lines of local overrides; all layout, cards, buttons, inputs, tables, and modal structures should use the global `.ht-*` classes.
2. **Tabular Numerals**: Always wrap market prices, asset counts, gain/loss numbers, and monetary figures in `.ht-tabular-num` to prevent jumping text when data streams update.
3. **No Cartoon Emojis**: Replace raw emoji icons in headers/buttons with clean vector SVGs or `.ht-badge` chips for an institutional, professional appearance.
