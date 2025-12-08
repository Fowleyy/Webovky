document.addEventListener('DOMContentLoaded', async function () {

    const CALENDAR_ID = window.CALENDAR_ID;
    const CAN_EDIT = window.CAN_EDIT;

    var calendarEl = document.getElementById('calendar');

    // ======================================================
    // LOAD EVENTS
    // ======================================================
    async function loadEvents() {
        const res = await fetch(`/api/events?calendarId=${CALENDAR_ID}`, {
            credentials: "same-origin"
        });

        if (!res.ok) {
            console.error("Events load failed:", res.status);
            return [];
        }
        return await res.json();
    }

    function createElement(html) {
        const template = document.createElement("template");
        template.innerHTML = html.trim();
        return template.content.firstChild;
    }

    // ======================================================
    // UPDATE MONTH TITLE
    // ======================================================
    function updateTitle() {
        const date = calendar.getDate();

        const formatter = new Intl.DateTimeFormat("cs-CZ", {
            month: "long",
            year: "numeric",
        });

        document.getElementById("cal-title").textContent =
            formatter.format(date);
    }

    // ======================================================
    // FULLCALENDAR CONFIG
    // ======================================================
    var config = {
        headerToolbar: false,
        initialView: "dayGridMonth",

        events: async (info, success) => {
            success(await loadEvents());
        },

        eventContent: function (arg) {

            let color =
                arg.event.extendedProps.color ||
                arg.event.backgroundColor ||
                "#6b46c1";

            let html = `
                <div style="display:flex;align-items:center;gap:6px;">
                    <span style="
                        width:10px;
                        height:10px;
                        display:inline-block;
                        border-radius:50%;
                        background:${color};
                    "></span>
                    <span>${arg.event.title}</span>
                </div>
            `;

            return { domNodes: [createElement(html)] };
        }
    };

    // EDIT PERMISSIONS
    if (CAN_EDIT) {
        config.dateClick = function (info) {
            window.location.href = `/event/create/${CALENDAR_ID}?start=` + info.dateStr;
        };

        config.eventClick = function (info) {
            window.location.href = `/event/edit/${info.event.id}`;
        };
    }

    var calendar = new FullCalendar.Calendar(calendarEl, config);
    calendar.render();
    calendar.refetchEvents();

    updateTitle(); // show current month

    // ======================================================
    // ACTIVE BUTTON HIGHLIGHT
    // ======================================================
    function setActive(buttonId) {
        const buttons = ["cal-day", "cal-week", "cal-month"];

        buttons.forEach(id => {
            document.getElementById(id).classList.remove("btn-purple");
            document.getElementById(id).classList.add("btn-outline-purple");
        });

        document.getElementById(buttonId).classList.remove("btn-outline-purple");
        document.getElementById(buttonId).classList.add("btn-purple");
    }

    // ======================================================
    // VIEW SWITCHING
    // ======================================================

    document.getElementById("cal-month").onclick = () => {
        calendar.changeView("dayGridMonth");
        setActive("cal-month");
        updateTitle();
    };

    document.getElementById("cal-week").onclick = () => {
        calendar.changeView("timeGridWeek");
        setActive("cal-week");
        updateTitle();
    };

    document.getElementById("cal-day").onclick = () => {
        calendar.changeView("timeGridDay");
        setActive("cal-day");
        updateTitle();
    };

    // ======================================================
    // MONTH SWITCHING
    // ======================================================

    document.getElementById("cal-prev").onclick = () => {
        calendar.prev();
        updateTitle();
    };

    document.getElementById("cal-next").onclick = () => {
        calendar.next();
        updateTitle();
    };

    // Default active button
    setActive("cal-month");
});
