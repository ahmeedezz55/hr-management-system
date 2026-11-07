// ==========================================
// 1. دوال فتح وإغلاق اللوحات
// ==========================================
function togglePanel(panelId) {
    const panel = document.getElementById(panelId);
    if (panel.style.display === 'none') {
        panel.style.display = 'block';
    } else {
        panel.style.display = 'none';
    }
}

function showCalcTab(tabName) {
    document.getElementById('calcTab-basic').style.display = 'none';
    document.getElementById('calcTab-salary').style.display = 'none';
    document.getElementById('calcTab-leave').style.display = 'none';
    document.getElementById('calcTab-' + tabName).style.display = 'block';
}

// ==========================================
// 2. الآلة الحاسبة
// ==========================================
let calcExpression = '';

function calcInput(value) {
    calcExpression += value;
    document.getElementById('calcDisplay').value = calcExpression;
}

function calcOp(operator) {
    calcExpression += ' ' + operator + ' ';
    document.getElementById('calcDisplay').value = calcExpression;
}

function calcClear() {
    calcExpression = '';
    document.getElementById('calcDisplay').value = '';
}

function calcResult() {
    try {
        const result = eval(calcExpression);
        calcExpression = result.toString();
        document.getElementById('calcDisplay').value = calcExpression;
    } catch (e) {
        document.getElementById('calcDisplay').value = 'خطأ';
        calcExpression = '';
    }
}

function calculateSalary() {
    const basic = parseFloat(document.getElementById('basicSalary').value) || 0;
    const allowances = parseFloat(document.getElementById('allowances').value) || 0;
    const deductions = parseFloat(document.getElementById('deductions').value) || 0;
    const netSalary = basic + allowances - deductions;
    document.getElementById('netSalary').textContent = netSalary.toFixed(2);
}

// ==========================================
// 3. حاسبة الإجازات
// ==========================================
function toggleDateType(type) {
    const select = document.getElementById(type + 'Type');
    const gregInput = document.getElementById(type + 'Date');
    const hijriDiv = document.getElementById(type + 'Hijri');
    
    if (select.value === 'hijri') {
        gregInput.style.display = 'none';
        hijriDiv.style.display = 'block';
    } else {
        gregInput.style.display = 'block';
        hijriDiv.style.display = 'none';
    }
}

function hijriToGregorian(hYear, hMonth, hDay) {
    const jd = Math.floor((11 * hYear + 3) / 30) + 354 * hYear + 30 * hMonth - Math.floor((hMonth - 1) / 2) + hDay + 1948440 - 385;
    const l = jd + 68569;
    const n = Math.floor(4 * l / 146097);
    const l2 = l - Math.floor((146097 * n + 3) / 4);
    const i = Math.floor(4000 * (l2 + 1) / 1461001);
    const l3 = l2 - Math.floor(1461 * i / 4) + 31;
    const j = Math.floor(80 * l3 / 2447);
    const day = l3 - Math.floor(2447 * j / 80);
    const l4 = Math.floor(j / 11);
    const month = j + 2 - 12 * l4;
    const year = 100 * (n - 49) + i + l4;
    return new Date(year, month - 1, day);
}

function calculateLeaveDays() {
    const startType = document.getElementById('startType').value;
    const endType = document.getElementById('endType').value;
    let startDate, endDate;
    
    if (startType === 'gregorian') {
        startDate = new Date(document.getElementById('startDate').value);
    } else {
        const hDay = parseInt(document.getElementById('startHDay').value);
        const hMonth = parseInt(document.getElementById('startHMonth').value);
        const hYear = parseInt(document.getElementById('startHYear').value);
        if (!hDay || !hMonth || !hYear) { alert('أدخل تاريخ البداية كاملاً'); return; }
        startDate = hijriToGregorian(hYear, hMonth, hDay);
    }
    
    if (endType === 'gregorian') {
        endDate = new Date(document.getElementById('endDate').value);
    } else {
        const hDay = parseInt(document.getElementById('endHDay').value);
        const hMonth = parseInt(document.getElementById('endHMonth').value);
        const hYear = parseInt(document.getElementById('endHYear').value);
        if (!hDay || !hMonth || !hYear) { alert('أدخل تاريخ النهاية كاملاً'); return; }
        endDate = hijriToGregorian(hYear, hMonth, hDay);
    }
    
    if (!startDate || !endDate || isNaN(startDate) || isNaN(endDate)) {
        alert('أدخل التواريخ بشكل صحيح');
        return;
    }
    
    const diffTime = endDate - startDate;
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
    
    document.getElementById('leaveDays').textContent = diffDays;
    
    if (diffDays < 0) {
        document.getElementById('leaveDays').style.color = '#ff6b6b';
    } else {
        document.getElementById('leaveDays').style.color = '#ffffff';
    }
}

// ==========================================
// 4. المساعد الذكي (Google Gemini)
// ==========================================
function addMessage(text, sender) {
    const chatMessages = document.getElementById('chatMessages');
    if (!chatMessages) return;
    
    const messageDiv = document.createElement('div');
    messageDiv.className = `chat-message ${sender}`;
    const time = new Date().toLocaleTimeString('ar-SA', { hour: '2-digit', minute: '2-digit' });
    
    // تحويل الأسطر الجديدة إلى <br> لعرضها بشكل صحيح
    messageDiv.innerHTML = `<div>${text.replace(/\n/g, '<br>')}</div><small>${time}</small>`;
    chatMessages.appendChild(messageDiv);
    
    const chatContainer = document.querySelector('.chat-container');
    if (chatContainer) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
}

async function sendMessage() {
    const input = document.getElementById('chatInput');
    const message = input.value.trim();
    if (!message) return;
    
    addMessage(message, 'user');
    input.value = '';
    
    // إظهار مؤشر "جاري الكتابة..."
    const typingDiv = document.createElement('div');
    typingDiv.className = 'chat-message ai';
    typingDiv.id = 'typingIndicator';
    typingDiv.innerHTML = '<div><i class="fas fa-spinner fa-spin"></i> المساعد يفكر...</div>';
    document.getElementById('chatMessages').appendChild(typingDiv);
    
    const chatContainer = document.querySelector('.chat-container');
    if (chatContainer) chatContainer.scrollTop = chatContainer.scrollHeight;

    try {
        const response = await fetch('/api/ai/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: message })
        });
        
        const data = await response.json();
        
        // إزالة مؤشر الكتابة
        const indicator = document.getElementById('typingIndicator');
        if (indicator) indicator.remove();
        
        if (data.reply) {
            addMessage(data.reply, 'ai');
        } else {
            addMessage(data.error || data.details || 'عذراً، حدث خطأ غير متوقع.', 'ai');
        }
    } catch (error) {
        const indicator = document.getElementById('typingIndicator');
        if (indicator) indicator.remove();
        addMessage('عذراً، فشل الاتصال بالمساعد الذكي. تأكد من تشغيل المشروع.', 'ai');
    }
}

// ==========================================
// 5. التهيئة عند تحميل الصفحة
// ==========================================
document.addEventListener('DOMContentLoaded', function() {
    const chatInput = document.getElementById('chatInput');
    if (chatInput) {
        chatInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault(); // منع إرسال النموذج الافتراضي
                sendMessage();
            }
        });
    }
    
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.getElementById('wrapper').classList.toggle('toggled');
        });
    }
    
    setTimeout(() => {
        addMessage("مرحباً! 👋 أنا المساعد الذكي المتقدم. اسألني عن أي شيء يخص الموارد البشرية، الإجازات، الرواتب، أو السياسات.", 'ai');
    }, 500);
});