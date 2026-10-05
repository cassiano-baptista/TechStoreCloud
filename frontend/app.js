const apiUrl = "https://app-techstorecloud-api-2026-g5emasgwc0cba2ah.mexicocentral-01.azurewebsites.net/api/produtos";

const formulario = document.getElementById("form-produto");
const campoId = document.getElementById("produto-id");
const campoNome = document.getElementById("nome");
const campoDescricao = document.getElementById("descricao");
const campoPreco = document.getElementById("preco");
const botaoCancelar = document.getElementById("botao-cancelar");
const lista = document.getElementById("lista-produtos");

async function carregarProdutos() {
    const resposta = await fetch(apiUrl);
    const produtos = await resposta.json();

    lista.innerHTML = "";

    produtos.forEach(produto => {
        const item = document.createElement("div");

        item.className = "produto";

        item.innerHTML = `
            <h3>${produto.nome}</h3>
            <p>${produto.descricao}</p>
            <p>R$ ${produto.preco.toFixed(2)}</p>

            <div class="acoes">
                <button class="botao-editar" onclick="editarProduto(${produto.id})">
                    Editar
                </button>

                <button class="botao-excluir" onclick="excluirProduto(${produto.id})">
                    Excluir
                </button>
            </div>
        `;

        lista.appendChild(item);
    });
}

formulario.addEventListener("submit", async function (event) {
    event.preventDefault();

    const id = campoId.value;

    const produto = {
        id: id ? Number(id) : 0,
        nome: campoNome.value,
        descricao: campoDescricao.value,
        preco: Number(campoPreco.value)
    };

    if (id) {
        await fetch(`${apiUrl}/${id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(produto)
        });
    } else {
        await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(produto)
        });
    }

    limparFormulario();
    await carregarProdutos();
});

async function editarProduto(id) {
    const resposta = await fetch(`${apiUrl}/${id}`);
    const produto = await resposta.json();

    campoId.value = produto.id;
    campoNome.value = produto.nome;
    campoDescricao.value = produto.descricao;
    campoPreco.value = produto.preco;
}

async function excluirProduto(id) {
    await fetch(`${apiUrl}/${id}`, {
        method: "DELETE"
    });

    await carregarProdutos();
}

function limparFormulario() {
    campoId.value = "";
    campoNome.value = "";
    campoDescricao.value = "";
    campoPreco.value = "";
}

botaoCancelar.addEventListener("click", function () {
    limparFormulario();
});

carregarProdutos();