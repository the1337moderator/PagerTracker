static class PagerTrackerPage
{
    public const string Html = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Pager Tracker</title>
  <style>
    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      min-height: 100vh;
      display: flex;
      justify-content: center;
      font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      background: #000;
      color: #172033;
    }

    .app {
      width: min(100vw - 64px, 1480px);
      padding: 24px 0 48px;
    }

    .title {
      margin: 0 0 30px;
      color: #fff;
      font-size: clamp(40px, 4vw, 56px);
      font-weight: 900;
      letter-spacing: 0;
      line-height: 1;
      text-align: center;
    }

    main {
      --button-size: clamp(82px, 10vh, 112px);
      --button-gap: 14px;
      --button-rows: 3;
      --panel-padding: 22px;
      --panel-heading-height: 21px;
      --panel-heading-gap: 22px;
      display: grid;
      grid-template-columns: 1fr 560px 1fr;
      gap: 28px;
      align-items: start;
    }

    section {
      min-width: 0;
      background: #fff;
      border: 1px solid #d8dee8;
      border-radius: 8px;
      box-shadow: 0 10px 28px rgba(15, 23, 42, 0.10);
      padding: var(--panel-padding);
      min-height: calc((var(--button-size) * var(--button-rows)) + (var(--button-gap) * (var(--button-rows) - 1)) + var(--panel-heading-height) + var(--panel-heading-gap) + (var(--panel-padding) * 2));
    }

    section:nth-child(2) {
      padding: 26px;
      min-height: calc((var(--button-size) * var(--button-rows)) + (var(--button-gap) * (var(--button-rows) - 1)) + var(--panel-heading-height) + var(--panel-heading-gap) + (var(--panel-padding) * 2));
    }

    h1, h2 {
      margin: 0 0 22px;
      color: #0f172a;
      font-size: 17px;
      font-weight: 800;
      letter-spacing: 0;
      line-height: 21px;
      text-transform: uppercase;
    }

    h1 {
      display: flex;
      align-items: center;
      justify-content: space-between;
      font-size: 24px;
      line-height: 29px;
    }

    .buttons {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      grid-auto-rows: var(--button-size);
      gap: var(--button-gap);
    }

    button {
      width: 100%;
      height: var(--button-size);
      border: 1px solid #cbd5e1;
      border-radius: 8px;
      background: #f8fafc;
      color: #0f172a;
      box-shadow: 0 2px 6px rgba(15, 23, 42, 0.08);
      font-size: clamp(30px, 3.2vh, 42px);
      font-weight: 800;
      cursor: pointer;
      touch-action: manipulation;
      user-select: none;
      transition: transform 120ms ease, border-color 120ms ease, box-shadow 120ms ease, background 120ms ease;
    }

    button:hover {
      border-color: #14b8a6;
      background: #ecfeff;
      box-shadow: 0 8px 18px rgba(15, 23, 42, 0.12);
      transform: translateY(-1px);
    }

    button:active {
      transform: translateY(0);
      box-shadow: 0 2px 6px rgba(15, 23, 42, 0.14);
    }

    #right button {
      background: #0f766e;
      border-color: #0f766e;
      color: #f8fafc;
      box-shadow: 0 8px 18px rgba(15, 118, 110, 0.22);
    }

    .table-wrap {
      overflow-x: auto;
      border: 1px solid #d8dee8;
      border-radius: 8px;
      background: #fff;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 22px;
    }

    th, td {
      padding: 17px 20px;
      border-bottom: 1px solid #e2e8f0;
      text-align: left;
    }

    th {
      background: #f8fafc;
      color: #475569;
      font-size: 14px;
      font-weight: 800;
      text-transform: uppercase;
    }

    tbody tr:last-child td {
      border-bottom: 0;
    }

    tbody tr:nth-child(even) {
      background: #f8fafc;
    }

    th:first-child,
    td:first-child {
      width: 84px;
      color: #0f766e;
      font-weight: 800;
    }

    td:last-child {
      color: #334155;
      font-variant-numeric: tabular-nums;
      white-space: nowrap;
    }

    @media (max-width: 1120px) {
      .app {
        width: calc(100vw - 28px);
        padding: 16px 0 28px;
      }

      .title {
        margin-bottom: 20px;
        font-size: clamp(34px, 7vw, 48px);
      }

      main {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 520px) {
      main {
        --button-size: 74px;
        --button-gap: 10px;
        --panel-padding: 16px;
      }

      section:nth-child(2) {
        padding: 16px;
      }

      table {
        font-size: 18px;
      }

      th, td {
        padding: 12px 14px;
      }
    }
  </style>
</head>
<body>
  <div class="app">
    <div class="title">PAGER TRACKER</div>
    <main>
      <section>
        <h2>Available</h2>
        <div id="left" class="buttons"></div>
      </section>

      <section>
        <h1>Checkout Log</h1>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Number</th>
                <th>Date / Time</th>
              </tr>
            </thead>
            <tbody id="log"></tbody>
          </table>
        </div>
      </section>

      <section>
        <h2>Active</h2>
        <div id="right" class="buttons"></div>
      </section>
    </main>
  </div>

  <script>
    const left = document.getElementById('left');
    const right = document.getElementById('right');
    const log = document.getElementById('log');

    loadState();
    setInterval(loadState, 2000);

    async function loadState() {
      const response = await fetch('/api/state');
      render(await response.json());
    }

    async function toggle(number) {
      const response = await fetch(`/api/toggle/${encodeURIComponent(number)}`, { method: 'POST' });
      render(await response.json());
    }

    function render(state) {
      left.replaceChildren();
      right.replaceChildren();
      log.replaceChildren();
      document.querySelector('main').style.setProperty('--button-rows', Math.max(1, Math.ceil((state.available.length + state.active.length) / 3)));

      for (const number of state.available) {
        left.appendChild(makeButton(number));
      }

      for (const entry of state.active) {
        right.appendChild(makeButton(entry.number));
        addLog(entry);
      }
    }

    function makeButton(number) {
      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = number;
      button.addEventListener('click', () => toggle(number));
      return button;
    }

    function addLog(entry) {
      const row = document.createElement('tr');
      const numberCell = document.createElement('td');
      const timeCell = document.createElement('td');

      numberCell.textContent = entry.number;
      timeCell.textContent = entry.time;

      row.appendChild(numberCell);
      row.appendChild(timeCell);
      log.appendChild(row);
    }
  </script>
</body>
</html>
""";
}
