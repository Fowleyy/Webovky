document.addEventListener("DOMContentLoaded", () => {

    console.log("notifications.js loaded!");

    const notifBtn = document.getElementById("notif-btn");
    const notifPanel = document.getElementById("notif-panel");
    const notifList = document.getElementById("notif-list");
    const notifBadge = document.getElementById("notif-badge");
    const notifSound = new Audio("/sounds/notif.mp3");

    let knownIds = new Set();

    notifBtn?.addEventListener("click", () => {
        notifPanel.style.display =
            notifPanel.style.display === "block" ? "none" : "block";
    });

    document.addEventListener("click", (e) => {
        if (!notifPanel.contains(e.target) && !notifBtn.contains(e.target)) {
            notifPanel.style.display = "none";
        }
    });

    async function loadNotifications() {
        try {
            const res = await fetch("/api/notifications");
            if (!res.ok) {
                notifList.innerHTML = "<div class='p-2 text-danger'>Chyba načítání</div>";
                return;
            }

            const items = await res.json();
            detectNewNotifications(items);
            renderNotifications(items);

        } catch (err) {
            console.error("Notif fetch error:", err);
        }
    }

    function detectNewNotifications(list) {
        const currentIds = new Set(list.map(n => n.id));

        for (let id of currentIds) {
            if (!knownIds.has(id)) {
                notifSound.play().catch(() => {});
            }
        }

        knownIds = currentIds;
    }

    function renderNotifications(list) {

        const unread = list.filter(n => !n.isRead).length;

        if (unread > 0) {
            notifBadge.innerText = unread;
            notifBadge.style.display = "inline-block";
        } else {
            notifBadge.style.display = "none";
        }

        if (list.length === 0) {
            notifList.innerHTML = "<div class='p-2'>Žádné notifikace</div>";
            return;
        }

        notifList.innerHTML = "";

        list.forEach(n => {
            const div = document.createElement("div");
            div.className = "notif-item " + (!n.isRead ? "unread" : "");

            div.innerHTML = `
                <div class="notif-text">
                    <strong>${n.title}</strong><br>
                    <small>${n.body}</small>
                </div>

                <button class="notif-delete-btn" data-id="${n.id}">×</button>
            `;

            div.addEventListener("click", () => markRead(n.id));

            notifList.appendChild(div);
        });

        // delete buttons
        document.querySelectorAll(".notif-delete-btn").forEach(btn => {
            btn.addEventListener("click", async (e) => {
                e.stopPropagation();
                const id = btn.getAttribute("data-id");

                await fetch(`/api/notifications/delete/${id}`, {
                    method: "DELETE"
                });

                loadNotifications();
            });
        });
    }


    async function markRead(id) {
        await fetch(`/api/notifications/read/${id}`, { method: "POST" });
        loadNotifications();
    }


    loadNotifications();
    setInterval(loadNotifications, 15000);
});
