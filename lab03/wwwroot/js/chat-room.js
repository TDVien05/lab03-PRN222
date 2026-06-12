(function () {
  const groupInput = document.getElementById("chat-group-id");
  const participantInput = document.getElementById("chat-participant-id");
  const displayNameInput = document.getElementById("chat-display-name");
  const form = document.getElementById("chat-form");
  const textArea = document.getElementById("chat-input");
  const sendButton = document.querySelector(".send-button");
  const attachImageButton = document.getElementById("attach-image-button");
  const imageInput = document.getElementById("chat-image-input");
  const attachFileButton = document.getElementById("attach-file-button");
  const fileInput = document.getElementById("chat-file-input");
  const messageList = document.getElementById("chat-messages");
  const status = document.getElementById("chat-status");
  const uploadProgress = document.getElementById("upload-progress");
  const uploadFileName = document.getElementById("upload-file-name");
  const uploadPercent = document.getElementById("upload-percent");
  const uploadProgressBar = document.getElementById("upload-progress-bar");
  const imagePreviewPanel = document.getElementById("image-preview-panel");
  const imagePreviewLink = document.getElementById("image-preview-link");
  const imagePreview = document.getElementById("image-preview");
  const imagePreviewName = document.getElementById("image-preview-name");
  const imagePreviewSize = document.getElementById("image-preview-size");
  const removeImageButton = document.getElementById("remove-image-button");
  const sendImageButton = document.getElementById("send-image-button");

  if (!groupInput || !participantInput || !form || !textArea || !sendButton || !messageList) {
    return;
  }

  const groupId = Number(groupInput.value);
  const participantId = Number(participantInput.value);
  const displayName = displayNameInput ? displayNameInput.value : "";
  let selectedImage = null;
  let selectedImageUrl = "";

  function setStatus(message) {
    if (status) {
      status.textContent = message;
    }
  }

  function getInitials(name) {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join("");
  }

  function formatFileSize(bytes) {
    if (bytes >= 1024 * 1024) {
      return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
    }

    if (bytes >= 1024) {
      return `${(bytes / 1024).toFixed(1)} KB`;
    }

    return `${bytes} B`;
  }

  function getVerificationToken() {
    const token = form.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : "";
  }

  function createMessageElement(message) {
    const isMine = Number(message.senderId) === participantId;
    const article = document.createElement("article");
    article.className = `stitch-message ${isMine ? "outgoing" : "incoming"}`;

    const avatar = document.createElement("div");
    avatar.className = "message-avatar text-avatar";
    avatar.textContent = getInitials(message.senderName || displayName);

    const stack = document.createElement("div");
    stack.className = "message-stack";

    const meta = document.createElement("div");
    meta.className = `message-line-meta ${isMine ? "outgoing-meta" : ""}`;

    const sender = document.createElement("strong");
    sender.textContent = message.senderName || displayName;

    const sentAt = document.createElement("span");
    sentAt.textContent = new Date(message.sentAt).toLocaleString();

    const bubble = document.createElement("div");
    bubble.className = "message-bubble-stitch";

    if (message.messageType === "image" && message.attachment) {
      const text = document.createElement("p");
      text.textContent = message.text || message.attachment.fileName;

      const link = document.createElement("a");
      link.className = "image-attachment";
      link.href = message.attachment.url;
      link.target = "_blank";
      link.rel = "noopener";

      const image = document.createElement("img");
      image.src = message.attachment.url;
      image.alt = message.attachment.fileName;

      link.append(image);
      bubble.append(text, link);
    } else if (message.messageType === "file" && message.attachment) {
      const link = document.createElement("a");
      link.className = "file-attachment";
      link.href = message.attachment.url;
      link.download = message.attachment.fileName;

      const fileIcon = document.createElement("span");
      fileIcon.className = "material-symbols-outlined";
      fileIcon.textContent = "description";

      const fileDetails = document.createElement("div");
      const fileName = document.createElement("strong");
      fileName.textContent = message.attachment.fileName;
      const fileSize = document.createElement("span");
      fileSize.textContent = formatFileSize(Number(message.attachment.fileSize || 0));

      const downloadIcon = document.createElement("span");
      downloadIcon.className = "material-symbols-outlined";
      downloadIcon.textContent = "download";

      fileDetails.append(fileName, fileSize);
      link.append(fileIcon, fileDetails, downloadIcon);
      bubble.append(link);
    } else {
      const text = document.createElement("p");
      text.textContent = message.text;
      bubble.append(text);
    }

    meta.append(sender, sentAt);
    stack.append(meta, bubble);
    article.append(avatar, stack);

    return article;
  }

  function removeEmptyState() {
    const emptyState = messageList.querySelector(".chat-empty-state");
    if (emptyState) {
      emptyState.remove();
    }
  }

  function scrollToBottom() {
    messageList.scrollTop = messageList.scrollHeight;
  }

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveMessage", (message) => {
    removeEmptyState();
    messageList.appendChild(createMessageElement(message));
    scrollToBottom();
  });

  connection.onreconnecting(() => {
    sendButton.disabled = true;
    setStatus("Reconnecting...");
  });

  connection.onreconnected(async () => {
    await connection.invoke("JoinGroup", groupId, participantId);
    sendButton.disabled = false;
    setStatus("");
  });

  connection.onclose(() => {
    sendButton.disabled = true;
    setStatus("Disconnected. Refresh the page to reconnect.");
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();

    const messageText = textArea.value.trim();
    if (!messageText) {
      return;
    }

    sendButton.disabled = true;
    setStatus("");

    try {
      await connection.invoke("SendMessage", groupId, participantId, messageText);
      textArea.value = "";
    } catch {
      setStatus("Message could not be sent.");
    } finally {
      if (connection.state === signalR.HubConnectionState.Connected) {
        sendButton.disabled = false;
      }
      textArea.focus();
    }
  });

  textArea.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      form.requestSubmit();
    }
  });

  if (attachImageButton && imageInput) {
    attachImageButton.addEventListener("click", () => {
      imageInput.click();
    });

    imageInput.addEventListener("change", () => {
      const file = imageInput.files && imageInput.files[0];
      if (!file) {
        return;
      }

      selectImage(file);
    });
  }

  if (attachFileButton && fileInput) {
    attachFileButton.addEventListener("click", () => {
      fileInput.click();
    });

    fileInput.addEventListener("change", () => {
      const file = fileInput.files && fileInput.files[0];
      if (!file) {
        return;
      }

      if (file.type && file.type.startsWith("image/")) {
        fileInput.value = "";
        selectImage(file);
        setStatus("Image selected. Preview it before sending.");
        return;
      }

      uploadFile(file, { clearImageAfterUpload: false });
    });
  }

  if (removeImageButton) {
    removeImageButton.addEventListener("click", () => {
      clearSelectedImage();
    });
  }

  if (sendImageButton) {
    sendImageButton.addEventListener("click", () => {
      if (selectedImage) {
        uploadFile(selectedImage, { clearImageAfterUpload: true });
      }
    });
  }

  function selectImage(file) {
    if (!file.type || !file.type.startsWith("image/")) {
      if (imageInput) {
        imageInput.value = "";
      }
      setStatus("Choose an image file.");
      return;
    }

    clearSelectedImage();
    selectedImage = file;
    selectedImageUrl = URL.createObjectURL(file);

    if (imagePreviewPanel && imagePreview && imagePreviewLink && imagePreviewName && imagePreviewSize) {
      imagePreview.src = selectedImageUrl;
      imagePreviewLink.href = selectedImageUrl;
      imagePreviewName.textContent = file.name;
      imagePreviewSize.textContent = formatFileSize(file.size);
      imagePreviewPanel.hidden = false;
    }

    setStatus("");
  }

  function clearSelectedImage() {
    if (selectedImageUrl) {
      URL.revokeObjectURL(selectedImageUrl);
    }

    selectedImage = null;
    selectedImageUrl = "";
    if (imageInput) {
      imageInput.value = "";
    }

    if (imagePreviewPanel && imagePreview && imagePreviewLink && imagePreviewName && imagePreviewSize) {
      imagePreviewPanel.hidden = true;
      imagePreview.removeAttribute("src");
      imagePreviewLink.href = "#";
      imagePreviewName.textContent = "";
      imagePreviewSize.textContent = "";
    }
  }

  function setUploadProgress(fileName, percent) {
    if (!uploadProgress || !uploadFileName || !uploadPercent || !uploadProgressBar) {
      return;
    }

    uploadProgress.hidden = false;
    uploadFileName.textContent = fileName;
    uploadPercent.textContent = `${percent}%`;
    uploadProgressBar.value = percent;
  }

  function finishUploadProgress(message) {
    setStatus(message);
    window.setTimeout(() => {
      if (uploadProgress) {
        uploadProgress.hidden = true;
      }
      setStatus("");
    }, 1600);
  }

  function uploadFile(file, options) {
    const uploadOptions = options || {};

    if (sendImageButton && uploadOptions.clearImageAfterUpload) {
      sendImageButton.disabled = true;
    }

    const formData = new FormData();
    formData.append("groupId", String(groupId));
    formData.append("participantId", String(participantId));
    formData.append("file", file);

    const request = new XMLHttpRequest();
    request.open("POST", "/ChatRoom?handler=Upload");

    const verificationToken = getVerificationToken();
    if (verificationToken) {
      request.setRequestHeader("RequestVerificationToken", verificationToken);
    }

    setStatus("");
    setUploadProgress(file.name, 0);

    request.upload.addEventListener("progress", (event) => {
      if (!event.lengthComputable) {
        return;
      }

      const percent = Math.round((event.loaded / event.total) * 100);
      setUploadProgress(file.name, percent);
    });

    request.addEventListener("load", () => {
      fileInput.value = "";

      if (request.status >= 200 && request.status < 300) {
        setUploadProgress(file.name, 100);
        if (uploadOptions.clearImageAfterUpload) {
          clearSelectedImage();
        }
        finishUploadProgress("File uploaded.");
        return;
      }

      try {
        const response = JSON.parse(request.responseText);
        finishUploadProgress(response.error || "File could not be uploaded.");
      } catch {
        finishUploadProgress("File could not be uploaded.");
      }
    });

    request.addEventListener("error", () => {
      fileInput.value = "";
      finishUploadProgress("File could not be uploaded.");
    });

    request.addEventListener("loadend", () => {
      if (sendImageButton && uploadOptions.clearImageAfterUpload) {
        sendImageButton.disabled = false;
      }
    });

    request.send(formData);
  }

  async function startChat() {
    try {
      sendButton.disabled = true;
      setStatus("Connecting...");
      await connection.start();
      await connection.invoke("JoinGroup", groupId, participantId);
      sendButton.disabled = false;
      setStatus("");
      scrollToBottom();
    } catch {
      setStatus("Real-time chat is unavailable. Refresh the page to try again.");
    }
  }

  startChat();
})();
