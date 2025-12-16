document.addEventListener("DOMContentLoaded", function () {

    const CALENDAR_ID = window.CALENDAR_ID;
    const CAN_EDIT = window.CAN_EDIT;

    if (!CALENDAR_ID) {
        console.error("CALENDAR_ID is missing");
        return;
    }

    const calendarEl = document.getElementById("calendar");
    if (!calendarEl) {
        console.error("Calendar element not found");
        return;
    }

    async function loadEvents() {
        const res = await fetch(`/api/events?calendarId=${CALENDAR_ID}`, {
            credentials: "include",
            headers: {
                "Accept": "application/json"
            }
        });

        if (!res.ok) {
            console.error("Events load failed:", res.status);
            return [];
        }

        const data = await res.json();

        // map backend Event → FullCalendar event
        return data.map(e => ({
            id: e.id,
            title: e.title,
            start: e.startTime,
            end: e.endTime,
            allDay: e.isAllDay === true,
            backgroundColor: e.color ?? "#6b46c1",
            borderColor: e.color ?? "#6b46c1",
            extendedProps: {
                description: e.description,
                location: e.location
            }
        }));
    }

    function createElement(html) {
        const template = document.createElement("template");
        template.innerHTML = html.trim();
        return template.content.firstChild;
    }

    function updateTitle() {
        const titleEl = document.getElementById("cal-title");
        if (!titleEl || !calendar) return;

        const date = calendar.getDate();

        const formatter = new Intl.DateTimeFormat("cs-CZ", {
            month: "long",
            year: "numeric",
        });

        titleEl.textContent = formatter.format(date);
    }



    const config = {
        initialView: "dayGridMonth",
        headerToolbar: false,

        events: async (info, successCallback, failureCallback) => {
            try {
                const events = await loadEvents();
                successCallback(events);
            } catch (err) {
                console.error("Event fetch error:", err);
                failureCallback(err);
            }
        },

        eventContent: function (arg) {
            const color =
                arg.event.backgroundColor ||
                arg.event.extendedProps?.color ||
                "#6b46c1";

            const html = `
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

    if (CAN_EDIT) {
        config.dateClick = function (info) {
            window.location.href =
                `/event/create/${CALENDAR_ID}?start=${info.dateStr}`;
        };

        config.eventClick = function (info) {
            window.location.href =
                `/event/edit/${info.event.id}`;
        };
    }

    const calendar = new FullCalendar.Calendar(calendarEl, config);
    calendar.render();
    calendar.refetchEvents();

    updateTitle(calendar);

    function setActive(buttonId) {
        const buttons = ["cal-day", "cal-week", "cal-month"];

        buttons.forEach(id => {
            const btn = document.getElementById(id);
            if (!btn) return;

            btn.classList.remove("btn-purple");
            btn.classList.add("btn-outline-purple");
        });

        const active = document.getElementById(buttonId);
        if (active) {
            active.classList.remove("btn-outline-purple");
            active.classList.add("btn-purple");
        }
    }

    document.getElementById("cal-month")?.addEventListener("click", () => {
        calendar.changeView("dayGridMonth");
        setActive("cal-month");
        updateTitle(calendar);
    });

    document.getElementById("cal-week")?.addEventListener("click", () => {
        calendar.changeView("timeGridWeek");
        setActive("cal-week");
        updateTitle(calendar);
    });

    document.getElementById("cal-day")?.addEventListener("click", () => {
        calendar.changeView("timeGridDay");
        setActive("cal-day");
        updateTitle(calendar);
    });

    document.getElementById("cal-prev")?.addEventListener("click", () => {
        calendar.prev();
        updateTitle(calendar);
    });

    document.getElementById("cal-next")?.addEventListener("click", () => {
        calendar.next();
        updateTitle(calendar);
    });

    setActive("cal-month");
});
