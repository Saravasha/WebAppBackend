window.PageEditor = (() => {
    let summernoteInstance;
    let debounceTimer;
    let allAssets = [];

    let basePath = "";
    let rootPath = "";

    function apiUrl(path) {
        return `${basePath}${path}`;
    }

    function init(config) {
        basePath = config.basePath || "";
        rootPath = config.rootPath || "";

        initSummernote();
        setupAssetModal();
        setupAssetSearch();
        $("#clearFiltersBtn").on("click", () => {
            $("#assetSearchForm")[0].reset();
            loadAssets();
        });
    }

    function getMimeType(fileUrl) {
        const ext = fileUrl.split(".").pop().toLowerCase();

        // Images
        if (["jpg", "jpeg", "png", "gif", "webp", "svg"].includes(ext)) {
            return `image/${ext === "jpg" ? "jpeg" : ext}`;
        }

        // Videos
        if (["mp4", "webm", "ogg"].includes(ext)) {
            if (ext === "mp4") return "video/mp4";
            if (ext === "webm") return "video/webm";
            if (ext === "ogg") return "video/ogg";
        }

        // Audio
        if (["mp3", "wav", "ogg"].includes(ext)) {
            if (ext === "mp3") return "audio/mpeg";
            if (ext === "wav") return "audio/wav";
            if (ext === "ogg") return "audio/ogg";
        }

        // Documents
        if (ext === "pdf") return "application/pdf";
        if (ext === "doc") return "application/msword";
        if (ext === "docx")
            return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        // Text
        if (["txt", "md"].includes(ext)) return "text/plain";

        // Fallback
        return "application/octet-stream";
    }

    function getAssetType(fileUrl) {
        if (/\.(jpe?g|png|gif|webp|svg)$/i.test(fileUrl)) return "image";

        if (/\.(mp4|webm|ogg)$/i.test(fileUrl)) return "video";

        if (/\.(mp3|wav|ogg)$/i.test(fileUrl)) return "audio";

        if (/\.(pdf|doc|docx)$/i.test(fileUrl)) return "document";

        if (/\.(txt|md)$/i.test(fileUrl)) return "text";

        return "other";
    }

    function buildAssetPreview(asset, fullUrl, mediaUrl, mimeType) {
        function getAssetCardAttributes(asset, fullUrl, mimeType) {
            return `
        tabindex="0"
        role="button"
        title="${asset.name}"
        aria-label="Insert ${asset.name} into editor"
        data-url="${fullUrl}"
        data-id="${asset.id}"
        data-type="${mimeType}"
    `;
        }

        switch (getAssetType(asset.fileUrl)) {
            case "image":
                return `
                       
                    <div class="asset-video-preview asset-card" ${getAssetCardAttributes(asset, fullUrl, mimeType)}>

                        <div class="asset-thumbnail">
                             <img src="${fullUrl}" class="asset-card-header img-fluid img-thumbnail asset-img">
                        </div>
                        <div class="asset-card-header">
                            <div class="asset-info">
                                <div class="asset-name" title="${asset.name}">
                                    ${asset.name}
                                </div>

                                <div class="asset-type">
                                    ${mimeType}
                                </div>
                            </div>
                        </div>
                    </div>
                    `;

            case "video":
                const thumb = asset.thumbnailUrl
                    ? window.location.origin +
                    "/" +
                    asset.thumbnailUrl.replace(/^\/+/, "")
                    : fullUrl;

                return `
        <div class="asset-video-preview asset-card" ${getAssetCardAttributes(asset, fullUrl, mimeType)}>

            <div class="asset-thumbnail">
                <img 
                    src="${thumb}" 
                    class="img-fluid img-thumbnail asset-img" />

                <i class="bi bi-play-circle-fill"></i>
            </div>

            <div class="asset-card-header">
                <div class="asset-info">
                    <div class="asset-name" title="${asset.name}">
                        ${asset.name}
                    </div>

                    <div class="asset-type">
                        ${mimeType}
                    </div>
                </div>
            </div>

        </div>
    `;

            case "audio":
                return `
                    <div class="asset-card" ${getAssetCardAttributes(asset, fullUrl, mimeType)}>

                        <div class="asset-card-header">

                            <div class="asset-icon">
                                🎵
                            </div>

                            <div class="asset-info">
                                <div 
                                    class="asset-name"
                                    title="${asset.name}">
                                    ${asset.name}
                                </div>

                                <div class="asset-type">
                                    ${mimeType}
                                </div>
                            </div>

                        </div>


                        <div class="audio-player">
                            <audio controls preload="metadata">
                                <source src="${mediaUrl}" type="${mimeType}">
                                Your browser does not support the audio element.
                            </audio>
                        </div>

                    </div>
                `;

            case "document":
                return `
                    <div class="document-preview asset-card" ${getAssetCardAttributes(asset, fullUrl, mimeType)}>

                        <div class="asset-thumbnail document-thumbnail">
                            <iframe 
                                src="${mediaUrl}"
                                loading="lazy">
                            </iframe>
                        </div>

                        <div class="asset-card-header">
                            <div class="asset-icon">
                                📄
                            </div>

                            <div class="asset-info">
                                <div class="asset-name" title="${asset.name}">
                                    ${asset.name}
                                </div>
                               <div class="asset-type">
                                    ${mimeType}
                                </div>
                            </div>
                        </div>

                    </div>
                `;

            case "text":
                return `
                    <div class="text-preview asset-card" ${getAssetCardAttributes(asset, fullUrl, mimeType)}>

                        <div class="asset-thumbnail document-thumbnail">
                            <iframe 
                                src="${mediaUrl}"
                                loading="lazy">
                            </iframe>
                        </div>

                        <div class="asset-card-header">
                            <div class="asset-icon">
                                📄
                            </div>

                            <div class="asset-info">
                                <div class="asset-name" title="${asset.name}">
                                    ${asset.name}
                                </div>
                               <div class="asset-type">
                                    ${mimeType}
                                </div>
                            </div>
                        </div>

                    </div>
                        `;
        }

        return "";
    }

    function buildSummernoteHtml(asset, mediaUrl, mimeType, downloadUrl) {
        switch (getAssetType(asset.fileUrl)) {
            case "image":
                return {
                    image: true,
                    url: asset.fileUrl,
                };

            case "video": {
                const poster = asset.thumbnailUrl
                    ? window.location.origin +
                    "/" +
                    asset.thumbnailUrl.replace(/^\/+/, "")
                    : "";

                return `
                        <video
                            controls
                            preload="none"
                            style="width:50%;"
                            ${poster ? `poster="${poster}"` : ""}
                        >
                            <source src="${mediaUrl}" type="${mimeType}">
                            Your browser does not support the video tag.
                        </video>
                    `;
            }
            case "audio":
                return `
                        <audio controls preload="none" style="width:50%">
                            <source src="${mediaUrl}" type="${mimeType}">
                        </audio>
                    `;

            case "document":
                return `
                        <figure class="asset-document"
                            data-asset-id="${asset.id}" contenteditable="false">
                            <iframe src="${mediaUrl}"    
                                style="width:100%;
                                height:800px;
                                border:none;">
                            </iframe>
                            <div class="asset-body">
                                <div class="asset-actions mt-3">
                                    <a href="${mediaUrl}" target="_blank" class="asset-open btn btn-primary">
                                        Open PDF
                                    </a>
                                    
                                    <a href="${downloadUrl}" download class="asset-download btn btn-secondary">
                                        Download
                                    </a>
                                </div>
                            </div>
                        </figure>
                        <p><br></p>
                    `;

            case "text":
                return `
                    <figure
                        class="asset-text"
                        data-asset-id="${asset.id}"
                        contenteditable="false">

                        <div
                            class="asset-render"
                            data-asset-id="${asset.id}">
                            ${asset.name}
                        </div>

                        <div class="asset-actions mt-3">
                            <a href="${mediaUrl}"
                               target="_blank"
                               class="asset-open btn btn-primary">
                                Open
                            </a>

                            <a href="${downloadUrl}"
                               download class="asset-download btn btn-secondary">
                                Download
                            </a>
                        </div>

                    </figure>

                    <p><br></p>
                `;
        }

        return "";
    }

    function getAssetDownloadUrl(asset) {
        const type = getAssetType(asset.fileUrl);

        if ((type == "document" || type === "text") && asset.id) {
            return apiUrl(`/Asset/Download/${asset.id}`);
        }
        return asset.fileUrl;
    }

    function getAssetMediaUrl(asset) {
        const type = getAssetType(asset.fileUrl);

        if ((type === "video" || type === "audio") && asset.id) {
            return apiUrl(`/Asset/Stream/${asset.id}`);
        }

        return asset.fileUrl;
    }

    async function getEditorFonts() {
        const response = await fetch(apiUrl(`/PageContent/GetFonts`));

        if (!response.ok) {
            return [];
        }

        const fonts = await response.json();

        return fonts;
    }

    function injectFonts(fonts) {

        const style = document.createElement("style");

        style.innerHTML = fonts.map(font => `
        @font-face {
            font-family: "${font.name}";
            src: url("${font.fileUrl}");
        }
    `).join("");

        document.head.appendChild(style);
    }

    async function initSummernote() {

        const fonts = await getEditorFonts();

        injectFonts(fonts);

        const summernoteFonts = [
            ...new Set([
                ...$.summernote.options.fontNames,
                ...fonts.map(x => x.name)
            ])
        ];

        $("#summernote").summernote({
            height: 300,

            fontNames: summernoteFonts,

            codeviewFilter: false,
            codeviewIframeFilter: false,
            callbacks: {
                onImageUpload(files) {
                    if (!files.length) return;
                    const file = files[0];
                    if (file.type.startsWith("image/")) {
                        uploadFile(file, "image");
                    } else if (file.type.startsWith("video/")) {
                        uploadFile(file, "video");
                    } else if (file.type.startsWith("audio/")) {
                        uploadFile(file, "audio");
                    } else if (file.type.startsWith("document/")) {
                        uploadFile(file, "document");
                    } else if (file.type.startsWith("text/")) {
                        uploadFile(file, "text");
                    } else {
                        alert("Unsupported file type.");
                    }
                },
            },
            toolbar: [
                ["style", ["style"]],
                ["font", ["bold", "italic", "underline", "clear"]],
                ["fontname", ["fontname"]],
                ["color", ["color"]],
                ["para", ["ul", "ol", "paragraph"]],
                ["height", ["height"]],
                ["table", ["table"]],
                ["insert", ["link", "picture", "video", "insertAsset"]],
                ["view", ["fullscreen", "codeview", "help"]],
            ],
            buttons: {
                insertAsset(context) {
                    const ui = $.summernote.ui;
                    summernoteInstance = context;
                    return ui
                        .button({
                            contents: '<i class="note-icon-picture"></i> Insert Asset',
                            tooltip: "Insert Asset from Asset Library",
                            click() {
                                $("#assetModal").modal("show");
                            },
                        })
                        .render();
                },
            },
        });

        $("#summernote")
            .next(".note-editor")
            .on("keydown", function (e) {
                if (e.key !== "Delete" && e.key !== "Backspace") return;

                const selected = $(".asset-selected");

                if (!selected.length) return;

                e.preventDefault();

                selected.remove();
            });
        $("#summernote")
            .next(".note-editor")
            .on("click", ".asset-document, .asset-text", function () {
                $(".asset-selected").removeClass("asset-selected");
                $(this).addClass("asset-selected");
            });
    }

    function uploadFile(file) {
        const formData = new FormData();
        formData.append("file", file);

        $.ajax({
            url: apiUrl(`/PageContent/UploadFile`),
            method: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success(data) {
                if (!data.url) {
                    alert("File upload failed.");
                    return;
                }

                const asset = {
                    id: data.id ?? 0,
                    name: data.name,
                    fileUrl: data.url,
                    thumbnailUrl: data.thumbnailUrl,
                };

                const mimeType = getMimeType(asset.fileUrl);
                const mediaUrl = getAssetMediaUrl(asset);
                const downloadUrl = getAssetDownloadUrl(asset);

                const html = buildSummernoteHtml(
                    asset,
                    mediaUrl,
                    mimeType,
                    downloadUrl,
                );

                if (html.image) {
                    summernoteInstance.invoke("editor.insertImage", html.url);
                } else {
                    summernoteInstance.invoke("editor.pasteHTML", html);
                }
            },
        });
    }
    function setupAssetModal() {
        $("#assetModal").on("shown.bs.modal", async () => {
            await loadCategories();
            loadAssets();
        });
    }

    function setupAssetSearch() {
        $("#assetSearchForm input, #assetSearchForm select")
            .off("input change")
            .on("input change", () => {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    const params = {};
                    $("#assetSearchForm")
                        .serializeArray()
                        .forEach(({ name, value }) => {
                            if (value?.trim()) params[name] = value.trim();
                        });
                    loadAssets(params);
                }, 300);
            });
    }

    async function loadCategories() {
        try {
            const res = await fetch(apiUrl("/api/react/categories"));
            const categories = await res.json();
            const select = $("#categorySelect");
            select.empty().append('<option value="">All Categories</option>');
            categories.forEach((cat) => {
                select.append(
                    `<option value="${(cat.name || "").toLowerCase()}">${cat.name}</option>`,
                );
            });
        } catch (err) {
            console.error("Failed to load categories:", err);
        }
    }

    function loadAssets(params = {}) {
        if (!allAssets.length) {
            fetch(apiUrl("/api/react/assets"))
                .then((res) => res.json())
                .then((data) => {
                    allAssets = data;
                    renderAssets(filterAssets(allAssets, params));
                })
                .catch((err) => console.error("Failed to load assets:", err));
        } else {
            renderAssets(filterAssets(allAssets, params));
        }
    }

    function filterAssets(assets, params) {
        return assets.filter((asset) => {
            if (
                params.name &&
                !asset.name?.toLowerCase().includes(params.name.toLowerCase())
            )
                return false;

            if (params.date) {
                const assetDate = new Date(asset.dateString).toISOString().slice(0, 10);
                const filterDate = new Date(params.date).toISOString().slice(0, 10);
                if (assetDate !== filterDate) return false;
            }

            if (params.category) {
                const catFilter = params.category.toLowerCase();
                if (
                    !asset.categories?.some(
                        (c) => (c.name || "").toLowerCase() === catFilter,
                    )
                )
                    return false;
            }
            return true;
        });
    }

    function renderAssets(assets) {
        const container = $("#assetContainer");
        $("#assetCount").text(
            `${assets.length} asset${assets.length !== 1 ? "s" : ""} found`,
        );
        container.empty();

        if (!assets.length) {
            container.append(`
                    <div class="col-12 text-center text-muted">
                        <p>No assets match your current filters.</p>
                        <button type="button" id="resetAfterEmpty" class="btn btn-sm btn-outline-secondary">Reset Filters</button>
                    </div>
                `);

            $("#resetAfterEmpty")
                .off("click")
                .on("click", () => {
                    $("#assetSearchForm")[0].reset();
                    loadAssets();
                });

            return;
        }

        const fullBaseUrl = window.location.origin + rootPath;

        assets.forEach((asset) => {
            if (!asset.fileUrl) return;

            const urlPath = asset.fileUrl.replace(/^\/+/, "");
            const fullUrl = fullBaseUrl + urlPath;
            const mediaUrl = getAssetMediaUrl(asset);
            const downloadUrl = getAssetDownloadUrl(asset);
            const fileUrl = asset.fileUrl;
            const mimeType = getMimeType(fileUrl);
            const type = getAssetType(asset.fileUrl);

            const mediaHtml = buildAssetPreview(asset, fullUrl, mediaUrl, mimeType);

            const assetDiv = $(`
                    <div class="col-md-3"">
                        ${mediaHtml}
                    </div>
                `);

            assetDiv.find(".asset-card").on("click keypress", (e) => {
                if (e.type === "click" || e.key === "Enter" || e.key === " ") {
                    const html = buildSummernoteHtml(
                        asset,
                        mediaUrl,
                        mimeType,
                        downloadUrl,
                    );

                    if (html.image) {
                        summernoteInstance.invoke("editor.insertImage", html.url);
                    } else {
                        summernoteInstance.invoke("editor.pasteHTML", html);
                    }

                    $("#assetModal").modal("hide");
                }
            });
            container.append(assetDiv);
        });
    }

    return {
        init,
    };
})();
