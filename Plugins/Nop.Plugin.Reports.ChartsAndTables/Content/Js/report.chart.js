/*
** nopCommercePlus Chart configure js
*/

function randomNum() {
    return Math.floor(Math.random() * 256);
}

function randomRGB() {
    var red = randomNum();
    var green = randomNum();
    var blue = randomNum();
    return [red, green, blue];
}

function colour(grade) {
    var rgbcur = this.randomRGB();
    var withgrade, withoutgrade = '';
    withgrade = "rgb(" + rgbcur[0] + ", " + rgbcur[1] + ", " + rgbcur[2] + ", " + grade + ")";
    withoutgrade = "rgb(" + rgbcur[0] + ", " + rgbcur[1] + ", " + rgbcur[2] + ")";
    return { withgrade, withoutgrade };
}

function RGB() {
    var rgbVals = randomRGB();
    var tempColor = "rgb(" + rgbVals[0] + ", " + rgbVals[1] + ", " + rgbVals[2] + ")";
    return tempColor;
}

function getDayNames(locales, format) {
    const weekList = [...Array(7).keys()];
    return weekList.map(dow => moment().locale(locales).weekday(dow).format(format));
}

function monthDays(lable) {
    var days = [];
    for (var i = 1; i <= 31; i++) {
        days.push(lable + " " + i);
    }
    return days;
}

function getMonthNames(locales, format) {
    const year = new Date().getFullYear(); // 2020
    const monthList = [...Array(12).keys()]; // [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]
    const formatter = new Intl.DateTimeFormat(locales, {
        month: format
    });

    const getMonthName = (monthIndex) =>
        formatter.format(new Date(year, monthIndex));

    return monthList.map(getMonthName);
}

var buttonsexport = function (e, dt, button, config) {
    var self = this;
    var oldStart = dt.settings()[0]._iDisplayStart;
    dt.one('preXhr', function (e, s, data) {
        // Just this once, load all data from the server...
        data.start = 0;
        data.length = 2147483647;
        dt.one('preDraw', function (e, settings) {
            // Call the original action function
            if (button[0].className.indexOf('buttons-excel') >= 0) {
                $.fn.dataTable.ext.buttons.excelHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-csv') >= 0) {
                $.fn.dataTable.ext.buttons.csvHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.csvHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.csvFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-pdf') >= 0) {
                $.fn.dataTable.ext.buttons.pdfHtml5.available(dt, config) ?
                    $.fn.dataTable.ext.buttons.pdfHtml5.action.call(self, e, dt, button, config) :
                    $.fn.dataTable.ext.buttons.pdfFlash.action.call(self, e, dt, button, config);
            } else if (button[0].className.indexOf('buttons-print') >= 0) {
                $.fn.dataTable.ext.buttons.print.action(e, dt, button, config);
            }
            dt.one('preXhr', function (e, s, data) {
                // DataTables thinks the first item displayed is index 0, but we're not drawing that.
                // Set the property to what it was before exporting.
                settings._iDisplayStart = oldStart;
                data.start = oldStart;
            });
            // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
            setTimeout(dt.ajax.reload, 0);
            // Prevent rendering of the full data to the DOM
            return false;
        });
    });
    // Requery the server with the new one-time export settings
    dt.ajax.reload();
}

$(function () {
    $('ul.nav-tabs > li.nav-item:first-child a:not(.active)').tab("show")
});

var nopChart = {
    DoubleLine: function (lablex, labley) {
        return {
            type: 'line',
            data: {
                labels: [],
                datasets: [
                    {
                        label: lablex,
                        fill: false,
                        data: []
                    },
                    {
                        label: labley,
                        fill: false,
                        data: []
                    }
                ]
            },
            options: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        fontSize: 12,
                    }
                },
                scales: {
                    x: {
                        display: true,
                        ticks: {
                            userCallback: function (dataLabel, index, values) {
                                if (values.length > 12) {
                                    return index % 5 === 0 ? dataLabel : '';
                                }
                                return dataLabel;
                            }
                        }
                    },
                    y: {
                        display: true,
                        ticks: {
                            userCallback: function (dataLabel, index) {
                                return (dataLabel ^ 0) === dataLabel ? dataLabel : '';
                            },
                            min: 0
                        },
                        type: 'linear'
                    }
                },
                showScale: true,
                scaleShowGridLines: false,
                scaleGridLineColor: "rgba(0,0,0,.03)",
                scaleGridLineWidth: 1,
                scaleShowHorizontalLines: true,
                scaleShowVerticalLines: true,
                bezierCurve: true,
                pointDot: false,
                pointDotRadius: 4,
                pointDotStrokeWidth: 1,
                pointHitDetectionRadius: 20,
                datasetStroke: true,
                datasetFill: true,
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    MultiBar: function (lables) {
        return {
            type: 'bar',
            data: {
                datasets: [
                    {
                        label: lables,
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: []
                    }
                ]
            },
            options: {
                title: { display: false },
                legend: {
                    position: 'bottom'
                },
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    PolarArea: function () {
        return {
            type: 'polarArea',
            data: {
                datasets: [
                    {
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: []
                    }
                ]
            },
            options: {
                title: { display: false },
                legend: {
                    position: "top",
                    labels: {
                        usePointStyle: true,
                        fontSize: 10
                    }
                },
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    AreaPie: function (lables) {
        return {
            type: 'pie',
            data: {
                datasets: [
                    {
                        label: lables,
                        fill: false,
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: []
                    }
                ]
            },
            options: {
                title: { display: false },
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        fontSize: 10
                    }
                },
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    LineBarMix: function (title, lablex, labley) {
        return {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    type: 'line',
                    label: lablex,
                    fill: false,
                    strokeColor: RGB(),
                    pointColor: RGB(),
                    pointStrokeColor: RGB(),
                    pointHighlightFill: RGB(),
                    pointHighlightStroke: RGB(),
                    pointBorderColor: RGB(),
                    pointBackgroundColor: RGB(),
                    pointHoverBackgroundColor: RGB(),
                    pointHoverBorderColor: RGB(),
                    pointBorderWidth: 1,
                    backgroundColor: RGB(),
                    borderColor: RGB(),
                    data: [],
                    yAxisID: 'right-axis'

                },
                {
                    label: labley,
                    fill: false,
                    strokeColor: RGB(),
                    pointColor: RGB(),
                    pointStrokeColor: RGB(),
                    pointHighlightFill: RGB(),
                    pointHighlightStroke: RGB(),
                    backgroundColor: [],
                    borderColor: [],
                    borderWidth: 1,
                    pointBorderColor: RGB(),
                    pointBackgroundColor: RGB(),
                    pointHoverBackgroundColor: RGB(),
                    pointHoverBorderColor: RGB(),
                    data: [],
                    yAxisID: 'left-axis'

                }
                ]
            },
            options: {
                legend: {
                    display: false
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 20
                    }
                },
                title: { display: true, text: title, fontSize: 13 },
                tooltips: { mode: 'index', intersect: true },
                hover: { mode: 'nearest', intersect: true },
                scales: {
                    x: {
                        display: false,
                        stacked: true,
                        scaleLabel: { display: false }
                    },
                    y: {
                        type: 'linear',
                        id: 'left-axis',
                        display: true,
                        position: 'left',
                        scaleLabel: { display: true, labelString: labley }
                    },
                    y2: {
                        type: 'linear',
                        id: 'right-axis',
                        display: true,
                        position: 'right',
                        stacked: false,
                        scaleLabel: { display: true, labelString: lablex },
                        gridLines: { drawOnChartArea: false }
                    }
                },
                showScale: true,
                scaleShowGridLines: false,
                scaleGridLineColor: "rgba(0,0,0,.03)",
                scaleGridLineWidth: 1,
                scaleShowHorizontalLines: true,
                scaleShowVerticalLines: true,
                bezierCurve: true,
                pointDot: false,
                pointDotRadius: 4,
                pointDotStrokeWidth: 1,
                pointHitDetectionRadius: 20,
                datasetStroke: true,
                datasetFill: true,
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    Bar: function (title) {
        return {
            type: 'bar',
            data: {
                labels: [],
                datasets: [
                    {
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: []
                    }
                ]
            },
            options: {
                title: { display: true, text: title, fontSize: 13 },
                legend: {
                    display: false
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 20
                    }
                },
                scales: {
                    x: {
                        display: false
                    },
                    y: {
                        display: true,
                        ticks: {
                            userCallback: function (dataLabel, index) {
                                return (dataLabel ^ 0) === dataLabel ? dataLabel : '';
                            },
                            min: 0
                        }
                    }
                },
                showScale: true,
                datasetStroke: true,
                datasetFill: true,
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    Pie: function (title) {
        return {
            type: 'pie',
            data: {
                datasets: [
                    {
                        fill: false,
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: []
                    }
                ]
            },
            options: {
                title: { display: true, text: title, fontSize: 13 },
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        fontSize: 10
                    }
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 20
                    }
                },
                maintainAspectRatio: false,
                responsive: true
            }
        };
    },
    DoublePie: function (title, lablex, labley) {
        return {
            type: 'pie',
            data: {
                datasets: [
                    {
                        fill: false,
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: [],
                        label: labley
                    },
                    {
                        fill: false,
                        backgroundColor: [],
                        borderColor: [],
                        borderWidth: 1,
                        data: [],
                        label: lablex
                    }
                ],
                labels: []
            },
            options: {
                title: { display: true, text: title, fontSize: 13 },
                legend: {
                    position: 'bottom',
                    labels: {
                        usePointStyle: true,
                        fontSize: 10
                    }
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 20
                    }
                },
                maintainAspectRatio: false,
                responsive: true,
                tooltips: {
                    callbacks: {
                        label: function (item, data) {
                            return data.datasets[item.datasetIndex].label + ": " + data.labels[item.index] + ": " + data.datasets[item.datasetIndex].data[item.index];
                        }
                    }
                }
            }
        };
    },
    Line: function (lables) {
        var fillcolour = colour("0.2");
        return {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    fillColor: fillcolour.withgrade,
                    strokeColor: RGB(),
                    pointColor: RGB(),
                    pointStrokeColor: RGB(),
                    pointHighlightFill: RGB(),
                    pointHighlightStroke: RGB(),
                    pointBorderColor: RGB(),
                    pointBackgroundColor: RGB(),
                    pointHoverBackgroundColor: RGB(),
                    pointHoverBorderColor: RGB(),
                    pointBorderWidth: 1,
                    backgroundColor: fillcolour.withgrade,
                    borderColor: RGB(),
                    data: []
                }]
            },
            options: {
                legend: {
                    display: false
                },
                layout: {
                    padding: {
                        left: 0,
                        right: 0,
                        top: 0,
                        bottom: 20
                    }
                },
                title: { display: true, text: lables, fontSize: 13 },
                scales: {
                    x: {
                        display: true,
                        ticks: {
                            userCallback: function (dataLabel, index) {
                                return dataLabel;
                            }
                        }
                    },
                    y: {
                        display: true,
                        ticks: {
                            userCallback: function (dataLabel, index) {
                                return (dataLabel ^ 0) === dataLabel ? dataLabel : '';
                            },
                            min: 0
                        }
                    }
                },
                showScale: true,
                scaleShowGridLines: false,
                scaleGridLineColor: "rgba(0,0,0,.03)",
                scaleGridLineWidth: 1,
                scaleShowHorizontalLines: true,
                scaleShowVerticalLines: true,
                bezierCurve: true,
                pointDot: false,
                pointDotRadius: 4,
                pointDotStrokeWidth: 1,
                pointHitDetectionRadius: 20,
                datasetStroke: true,
                datasetFill: true,
                maintainAspectRatio: false,
                responsive: true
            }
        };
    }
};