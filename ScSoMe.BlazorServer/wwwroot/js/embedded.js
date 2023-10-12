let embedded = {
    Url: null,
    Img: null,
    Title: null
}

function createEmbedded(urlParam, imgParam, titleParam, quill) {
    console.log("FROM embedded.js");
    if (urlParam == null || imgParam == null || titleParam == null) return;
    setEmbedded(urlParam, imgParam, titleParam);
    const link = document.createElement('a');
    link.href = urlParam;
    link.target = '_blank';
    const linkChild = document.createElement('div');
    linkChild.classList.add('embedded-link');
    const imgContainer = document.createElement('div');
    imgContainer.classList.add('embedded-link-img');
    const img = document.createElement('img');
    img.src = imgParam;
    img.alt = 'Embedded link image';
    imgContainer.appendChild(img);
    linkChild.appendChild(imgContainer);
    const textContainer = document.createElement('div');
    textContainer.classList.add('embedded-link-text');
    const title = document.createElement('p');
    title.classList.add('embedded-link-text__title');
    title.innerText = titleParam;
    textContainer.appendChild(title);
    const url = document.createElement('p');
    url.classList.add('embedded-link-text__url');
    url.innerText = urlParam;
    textContainer.appendChild(url);
    linkChild.appendChild(textContainer);
    link.appendChild(linkChild);
    const linkContainer = document.createElement('div');
    const closeBtn = document.createElement('button');
    linkContainer.classList.add('embedded-link-container');
    linkContainer.appendChild(closeBtn);
    linkContainer.appendChild(link);
    closeBtn.innerHTML = '&#10006;';
    closeBtn.classList.add('embedded-close-btn');
    closeBtn.addEventListener('click', () => {
        clearEmbedded(quill);
    });
    quill.container.appendChild(linkContainer);
}

function clearEmbedded(quill) {
    let embeddedContainer = quill.container.querySelector(".embedded-link-container");
    if (embeddedContainer) {
        quill.container.removeChild(embeddedContainer);
        setEmbedded(null, null, null);
    }
}

function getEmbedded() {
    return embedded;
}

function setEmbedded(url, img, title) {
    embedded = {
        Url: url,
        Img: img,
        Title: title
    };
}