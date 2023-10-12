(function () {
    async function suggestPeople(searchTerm) {
        if (!searchTerm || searchTerm === '') return;
        // change this url when deploying
        const URL = 'https://api.startupcentral.dk/Members/SearchMembers?';
        //const URL = 'https://localhost:7297/Members/SearchMembers?';
        try {
            const response = await fetch(URL + new URLSearchParams({
                terms: searchTerm
            }), {
                method: 'GET',
                headers: {
                    Accept: 'application/json'
                }
            });
            const matched = await response.json();
            return matched;
        } catch (err) {
            console.log(err);
        }
    }
    var Delta = Quill.import('delta');
    // let embeddedUrl, embeddedImg, embeddedTitle;
    window.QuillFunctions = {
        createQuill: function (
            quillElement, _toolBar, _hasToolBar, readOnly,
            placeholder, _theme, debugLevel) {
            var toolbarOptions = {
                container: [
                    [{ 'header': [1, 2, 3, false] }],
                    ['bold', 'italic', 'underline'],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    [{ 'indent': '-1' }, { 'indent': '+1' }],
                    [{ 'align': [] }],
                    ['link']
                ],
            }
            var options = {
                modules: {
                    toolbar: (_hasToolBar) ? toolbarOptions : false,
                    magicUrl: true,
                    mention: {
                        allowedChars: /^[A-Za-z\sÅÄÖåäö_]*$/,
                        //allowedChars: /^[a-zA-Z0-9\u00c0-\u017e]*$/,
                        mentionDenotationChars: ["@"],
                        showDenotationChar: false,
                        defaultMenuOrientation: 'bottom',
                        source: async function (searchTerm, renderList) {
                            await suggestPeople(searchTerm).then(matched => {
                                if (matched) {
                                    // change object key names
                                    const mappedArr = matched.map(e => ({ id: e.id, value: e.name }));
                                    renderList(mappedArr);
                                }
                            });
                        }
                    }
                },
                placeholder: placeholder,
                //readOnly: readOnly,

                //placeholder: placeholder,
                readOnly: false,
                theme: _theme
            };
            var quill = new Quill(quillElement, options);
            quill.focus();



            // Store accumulated changes
            var change = new Delta();
            let cachedMatch;
            const ytRegex = /((?: https ?:) ?\/\/)?((?:www|m)\.)?((?:youtube(-nocookie)?\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?/;
            const imgRegex = /https?:\/\/.+\.(jpg|jpeg|png|webp|avif|gif|svg)/;
            const urlRegex = /(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})/;
            quill.on('text-change', function (delta, oldDelta, source) {
                change = change.compose(delta);
                if (source == 'user') {
                    console.log("ONTEXTCHANGE: ");
                    console.log(quill.getLength());
                    // if user clears editor, clear cached value
                    if (quill.getLength() <= 2) cachedMatch = null;
                    let matchVideo = quill.getText().match(ytRegex);
                    let matchImg = quill.getText().match(imgRegex);
                    let matchUrl = quill.getText().match(urlRegex);
                    if (matchVideo) {
                        // quill.deleteText(matchVideo.index, matchVideo[0].length);
                        if (matchVideo[6] == cachedMatch) return;
                        quill.insertEmbed(quill.getLength() - 1, 'video', 'https://www.youtube.com/embed/' + matchVideo[6]);
                        cachedMatch = matchVideo[6];
                    } else if (matchImg) {
                        // quill.deleteText(matchImg.index, matchImg[0].length);
                        if (matchImg[0] == cachedMatch) return;
                        quill.insertEmbed(quill.getLength() - 1, 'image', matchImg[0]);
                        cachedMatch = matchImg[0];
                    } else if (matchUrl) {
                        console.log("URL FOUND:");
                        // REPLACE URL IN PRODUCTION
                        fetch('https://api.startupcentral.dk/Comments/GetUrlMetadata?url=' + matchUrl[0])
                            .then(function (response) {
                                return response.json();
                            }).then(function (resp) {
                                if (resp != null) {
                                    clearEmbedded(quill);
                                    createEmbedded(matchUrl[0], (resp.meta.image != null) ? resp.meta.image : resp.meta.site.logo, resp.meta.title, quill);
                                }
                            });
                    }
                }
            });
            setInterval(function () {
                if (change.length() > 0) {
                    console.log('Saving changes', change);
                    // Save the entire updated text to localStorage
                    const data = JSON.stringify(quill.getContents());
                    localStorage.setItem('storedText', data);
                    change = new Delta();
                }
            }, 5 * 1000);


            // Check for unsaved data
            window.onbeforeunload = function () {
                if (change.length() > 0) {
                    return 'There are unsaved changes. Are you sure you want to leave?';
                }
            }

        },
        getQuillContent: function (quillElement) {
            return JSON.stringify(quillElement.__quill.getContents());
        },
        getQuillText: function (quillElement) {
            return quillElement.__quill.getText();
        },
        setQuillText: function (quillElement, quillContent) {
            quillElement.__quill.setText(quillContent);
        },
        getQuillHTML: function (quillElement) {
            return quillElement.__quill.root.innerHTML;
        },
        loadQuillContent: function (quillElement, quillContent) {
            content = JSON.parse(quillContent);
            return quillElement.__quill.setContents(content, 'api');
        },
        getQuillSelection: function (quillElement) {
            return JSON.stringify(quillElement.__quill.getSelection());
        },
        setQuillSelection: function (quillElement, index, length) {
            quillElement.__quill.setSelection(index, length);
        },
        quillFocus: function (quillElement) {
            quillElement.__quill.focus();
        },
        insertQuillText: function (quillElement, index, quillContent) {
            return quillElement.__quill.insertText(index, quillContent, 'api');
        },
        enableQuillEditor: function (quillElement, mode) {
            quillElement.__quill.enable(mode);
        },
        getQuillEmbedded: function () {
            console.log(getEmbedded());
            return JSON.stringify(getEmbedded());
        },
        createQuillEmbedded: function (quillElement, url, img, title) {
            createEmbedded(url, img, title, quillElement.__quill);
        },
        clearQuillEmbedded: function (quillElement) {
            clearEmbedded(quillElement.__quill);
        }
    };
})();