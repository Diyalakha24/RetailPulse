// RetailPulse dashboard: renders Chart.js charts from server-provided data
// and re-fetches KPIs/charts/insights via a small JSON endpoint whenever a
// filter changes, so the dashboard updates without a full page reload.
(function () {
    "use strict";

    if (!document.getElementById("dashboardContent")) {
        // No data at all - nothing to wire up.
        return;
    }

    var PALETTE = ["#2f6fed", "#1a9e6d", "#f2a93b", "#d64545", "#7c5cfc", "#0ea5b7"];
    var CURRENCY_FORMATTER = new Intl.NumberFormat("en-ZA", {
        style: "currency",
        currency: "ZAR",
        maximumFractionDigits: 0
    });
    var NUMBER_FORMATTER = new Intl.NumberFormat("en-ZA");

    var charts = {};

    function paletteColor(index) {
        return PALETTE[index % PALETTE.length];
    }

    function initCharts(data) {
        var ctxTrend = document.getElementById("revenueTrendChart");
        charts.trend = new Chart(ctxTrend, {
            type: "line",
            data: {
                labels: data.revenueOverTime.map(function (p) { return p.label; }),
                datasets: [{
                    label: "Revenue",
                    data: data.revenueOverTime.map(function (p) { return p.value; }),
                    borderColor: PALETTE[0],
                    backgroundColor: "rgba(47, 111, 237, 0.12)",
                    tension: 0.3,
                    fill: true,
                    pointRadius: 3
                }]
            },
            options: baseOptions(true)
        });

        var ctxCategory = document.getElementById("categoryChart");
        charts.category = new Chart(ctxCategory, {
            type: "doughnut",
            data: {
                labels: data.revenueByCategory.map(function (p) { return p.label; }),
                datasets: [{
                    data: data.revenueByCategory.map(function (p) { return p.value; }),
                    backgroundColor: data.revenueByCategory.map(function (_, i) { return paletteColor(i); })
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: "bottom" } }
            }
        });

        var ctxTop = document.getElementById("topProductsChart");
        charts.topProducts = new Chart(ctxTop, {
            type: "bar",
            data: {
                labels: data.topProducts.map(function (p) { return p.label; }),
                datasets: [{
                    label: "Revenue",
                    data: data.topProducts.map(function (p) { return p.value; }),
                    backgroundColor: PALETTE[0]
                }]
            },
            options: baseOptions(false, true)
        });

        var ctxRegion = document.getElementById("regionChart");
        charts.region = new Chart(ctxRegion, {
            type: "bar",
            data: {
                labels: data.revenueByRegion.map(function (p) { return p.label; }),
                datasets: [{
                    label: "Revenue",
                    data: data.revenueByRegion.map(function (p) { return p.value; }),
                    backgroundColor: PALETTE[1]
                }]
            },
            options: baseOptions(true)
        });
    }

    function baseOptions(showLegend, horizontal) {
        return {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: horizontal ? "y" : "x",
            plugins: {
                legend: { display: !!showLegend && false },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return " " + CURRENCY_FORMATTER.format(context.parsed.y ?? context.parsed.x ?? context.parsed);
                        }
                    }
                }
            },
            scales: {
                x: { ticks: { callback: horizontal ? function (v) { return CURRENCY_FORMATTER.format(v); } : undefined } },
                y: { ticks: { callback: !horizontal ? function (v) { return CURRENCY_FORMATTER.format(v); } : undefined } }
            }
        };
    }

    function updateCharts(data) {
        charts.trend.data.labels = data.revenueOverTime.map(function (p) { return p.label; });
        charts.trend.data.datasets[0].data = data.revenueOverTime.map(function (p) { return p.value; });
        charts.trend.update();

        charts.category.data.labels = data.revenueByCategory.map(function (p) { return p.label; });
        charts.category.data.datasets[0].data = data.revenueByCategory.map(function (p) { return p.value; });
        charts.category.data.datasets[0].backgroundColor = data.revenueByCategory.map(function (_, i) { return paletteColor(i); });
        charts.category.update();

        charts.topProducts.data.labels = data.topProducts.map(function (p) { return p.label; });
        charts.topProducts.data.datasets[0].data = data.topProducts.map(function (p) { return p.value; });
        charts.topProducts.update();

        charts.region.data.labels = data.revenueByRegion.map(function (p) { return p.label; });
        charts.region.data.datasets[0].data = data.revenueByRegion.map(function (p) { return p.value; });
        charts.region.update();
    }

    function updateKpis(kpis) {
        document.getElementById("kpiTotalRevenue").textContent = CURRENCY_FORMATTER.format(kpis.totalRevenue);
        document.getElementById("kpiUnitsSold").textContent = NUMBER_FORMATTER.format(kpis.unitsSold);
        document.getElementById("kpiAov").textContent = CURRENCY_FORMATTER.format(kpis.averageOrderValue);
        document.getElementById("kpiTopProduct").textContent = kpis.topProduct;
    }

    function updateInsights(insights) {
        var list = document.getElementById("insightsList");
        list.innerHTML = "";

        insights.forEach(function (insight) {
            var directionClass = insight.direction === 1 ? "positive" : (insight.direction === 2 ? "negative" : "neutral");
            var icon = directionClass === "positive" ? "↑" : (directionClass === "negative" ? "↓" : "•");

            var li = document.createElement("li");
            li.className = "rp-insight-item " + directionClass;
            li.innerHTML = '<span class="rp-insight-icon">' + icon + '</span><span>' + insight.text + '</span>';
            list.appendChild(li);
        });
    }

    function applyDashboardData(data) {
        var content = document.getElementById("dashboardContent");
        var noResults = document.getElementById("noResultsMessage");

        if (!data.hasData) {
            content.classList.add("d-none");
            noResults.classList.remove("d-none");
            return;
        }

        content.classList.remove("d-none");
        noResults.classList.add("d-none");

        updateKpis(data.kpis);
        updateInsights(data.insights);

        if (charts.trend) {
            updateCharts(data);
        } else {
            initCharts(data);
        }
    }

    function fetchFiltered() {
        var params = new URLSearchParams(new FormData(document.getElementById("filterForm")));
        var url = window.retailPulseFilterUrl + "?" + params.toString();

        fetch(url, { headers: { "Accept": "application/json" } })
            .then(function (response) { return response.json(); })
            .then(applyDashboardData)
            .catch(function () {
                // A failed refresh should not crash the page - the dashboard
                // simply keeps showing the last known good data.
            });
    }

    // Initial render from the data embedded by the server on page load.
    applyDashboardData(window.retailPulseInitialData);

    var form = document.getElementById("filterForm");
    if (form) {
        form.addEventListener("change", fetchFiltered);
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            fetchFiltered();
        });

        var resetButton = document.getElementById("resetFilters");
        if (resetButton) {
            resetButton.addEventListener("click", function () {
                form.reset();
                fetchFiltered();
            });
        }
    }
})();
