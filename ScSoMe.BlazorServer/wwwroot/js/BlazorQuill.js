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
    var files;

    // apparently JSON.stringify() doesn't work for File objects 
    const stringifyFiles = (files) => {
        const convertedFiles = [];
        if (files.length !== 0) {
            for (let f of files) {
                // reCreate new Object and set File Data into it
                var newObject = {
                    'lastModified': f.lastModified,
                    'lastModifiedDate': f.lastModifiedDate,
                    'name': f.name,
                    'size': f.size,
                    'type': f.type
                };
                convertedFiles.push(newObject);
            }
        }

        return JSON.stringify(convertedFiles);
    };

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

            // Button for attaching files

//            const attachIcon = `<?xml version="1.0" encoding="iso-8859-1"?>
//<!-- Generator: Adobe Illustrator 19.0.0, SVG Export Plug-In . SVG Version: 6.00 Build 0)  -->
//<svg version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px"
//	 viewBox="0 0 280.067 280.067" style="enable-background:new 0 0 280.067 280.067;" xml:space="preserve">
//<g>
//	<path style="fill:#000000;" d="M149.823,257.142c-31.398,30.698-81.882,30.576-113.105-0.429
//		c-31.214-30.987-31.337-81.129-0.42-112.308l-0.026-0.018L149.841,31.615l14.203-14.098c23.522-23.356,61.65-23.356,85.172,0
//		s23.522,61.221,0,84.586l-125.19,123.02l-0.044-0.035c-15.428,14.771-40.018,14.666-55.262-0.394
//		c-15.244-15.069-15.34-39.361-0.394-54.588l-0.044-0.053l13.94-13.756l69.701-68.843l13.931,13.774l-83.632,82.599
//		c-7.701,7.596-7.701,19.926,0,27.53s20.188,7.604,27.88,0L235.02,87.987l-0.035-0.026l0.473-0.403
//		c15.682-15.568,15.682-40.823,0-56.39s-41.094-15.568-56.776,0l-0.42,0.473l-0.026-0.018l-14.194,14.089L50.466,158.485
//		c-23.522,23.356-23.522,61.221,0,84.577s61.659,23.356,85.163,0l99.375-98.675l14.194-14.089l14.194,14.089l-14.194,14.098
//		l-99.357,98.675C149.841,257.159,149.823,257.142,149.823,257.142z"/>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//<g>
//</g>
//</svg>
//`;
//            const hiddenIn = document.querySelector('#fileId');
//            const attachBtn = new QuillToolbarButton({
//                icon: attachIcon
//            });


            // show file picker
            //attachBtn.onClick = function (quill) {
            //    if (hiddenIn != null) {
            //        hiddenIn.click();
            //    }
            //}

            //hiddenIn.addEventListener('change', e => {
            //    files = e.target.files;
            //    console.log("FROM JS IN CHANGE EVENT: ");
            //    console.log(files);
            //});

            //attachBtn.attach(quill);
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
                    //console.log("ONTEXTCHANGE: ");
                    //console.log(quill.getLength());
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
                        //console.log("URL FOUND:");
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
                    //console.log('Saving changes', change);
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
            //console.log(getEmbedded());
            return JSON.stringify(getEmbedded());
        },
        createQuillEmbedded: function (quillElement, url, img, title) {
            createEmbedded(url, img, title, quillElement.__quill);
        },
        clearQuillEmbedded: function (quillElement) {
            clearEmbedded(quillElement.__quill);
        },
        //getQuillAttachedFiles: function (quillElement) {
        //    console.log("FROM JS IN METHOD: ");
        //    console.log(files);
        //    console.log(stringifyFiles(files));
        //    return stringifyFiles(files);
        //}
    };
})();