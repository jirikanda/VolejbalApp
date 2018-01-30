export const ensureTextContentShortened = (textContentDiv: HTMLDivElement, countOfLinesToShow: number, lineHeight: number, text: string) => {
    let isTextContentShortened = false;

    if (textContentDiv && typeof text === "string") {
        const wordArray = text.split(' ');
        textContentDiv.style.height = `${lineHeight * countOfLinesToShow}em`;
        while (textContentDiv.scrollHeight > textContentDiv.offsetHeight) {
            wordArray.pop();
            textContentDiv.innerHTML = `${wordArray.join(' ')}...`;
            isTextContentShortened = true;
        }
    }

    return isTextContentShortened;
}