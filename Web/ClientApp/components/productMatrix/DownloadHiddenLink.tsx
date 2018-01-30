import * as React from 'react';
import { Tooltip, UncontrolledTooltip } from 'reactstrap';

interface DownloadHiddenLinkProps {
    PDFUrl: string;
    resetPDF: () => void;
}

class DownloadHiddenLink extends React.Component<DownloadHiddenLinkProps> {
    linkRef: HTMLAnchorElement = null;

    componentDidMount() {
        this.linkRef.click();
        this.props.resetPDF();
    }

    render() {
        const {PDFUrl} = this.props;

        return <a href={PDFUrl} ref={ref => this.linkRef = ref} download={"matice_produktu.pdf"} />;
    }
}

export default DownloadHiddenLink;