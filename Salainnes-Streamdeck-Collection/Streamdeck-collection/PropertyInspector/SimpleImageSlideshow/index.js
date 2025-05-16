
function refreshSettings(settings) {
    var text_url = document.getElementById('text_url');
    var select_interval = document.getElementById('select_interval');

    if (settings) {
        select_interval.value = settings.UpdateInterval;
        text_url.value = settings.Url;
    }
    text_url.disabled = false;
    select_interval.disabled = false;
}

function updateSettings() {
    var text_url = document.getElementById('text_url');
    var select_interval = document.getElementById('select_interval');

    var setSettings = {};
    setSettings.event = 'setSettings';
    setSettings.context = uuid;
    setSettings.payload = {};
    setSettings.payload.UpdateInterval = select_interval.value;
    setSettings.payload.Url = text_url.value;

    websocket.send(JSON.stringify(setSettings));
}