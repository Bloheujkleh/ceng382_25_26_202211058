document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("classForm");
    const table = document.getElementById("classTable").querySelector("tbody");

    let classEntries = [];

    // 🎯 Form Submit Event: Yeni veri ekleme
    form.addEventListener("submit", function (event) {
        event.preventDefault();

        let className = document.getElementById("className").value.trim();
        let numPeople = document.getElementById("numPeople").value.trim();
        let description = document.getElementById("description").value.trim();

        if (className === "" || numPeople === "" || description === "") {
            alert("Tüm alanlari doldurun!");
            return;
        }

        let newRow = document.createElement("tr");
        newRow.innerHTML = `
            <td>${className}</td>
            <td>${numPeople}</td>
            <td>${description}</td>
        `;
        // Satırı tabloya ekle
        table.appendChild(newRow);

        // Dizimize ekleyelim
        classEntries.push({ className, numPeople, description });

        // Formu sıfırla
        form.reset();
    });

    //  Input Focus Event: Giriş kutusu aktifken stil değiştir
    document.querySelectorAll("input").forEach(input => {
        input.addEventListener("focus", function () {
            this.style.borderColor = "#4CAF50";
            this.style.boxShadow = "0 0 8px rgba(76, 175, 80, 0.5)";
        });

        //  Input Blur Event: Odaktan çıkınca doğrulama yap
        input.addEventListener("blur", function () {
            this.style.borderColor = "#ccc";
            this.style.boxShadow = "none";

            if (this.value.trim() === "") {
                this.style.borderColor = "red";
            }
        });

        // 🎯 Keyup Event: Gerçek zamanlı doğrulama
        input.addEventListener("keyup", function () {
            const hasUpperCase = /[A-Z]/.test(this.value);  // Büyük harf kontrolü
            if (hasUpperCase) {
                this.style.borderColor = "#4CAF50";  // Yeşil border (büyük harf var)
            } else {
                this.style.borderColor = "red";  // Kırmızı border (büyük harf yok)
            }
        });
        
    });
});
