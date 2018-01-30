import * as React from 'react';
import { ButtonDropdown, DropdownItem, DropdownMenu, DropdownToggle } from 'reactstrap';

import { PdfTemplate } from '../../apimodels/ProductMatrixStore';
import { Toastr } from '../../store/commonActionCreators/Toaster';
import DownloadHiddenLink from './DownloadHiddenLink';

interface ExportToPDFButtonProps {
    columnsCount: number;
    PDFUrl: string;
    exportToPDF: (template: PdfTemplate) => void;
    resetPDF: () => void;
}

interface ExportToPDFButtonState {
    isDropdownOpen: boolean;
}

const maxAllowedColumnsCount = 3;

class ExportToPDFButton extends React.Component<ExportToPDFButtonProps, ExportToPDFButtonState> {
    constructor(props) {
        super(props);

        this.state = {
            isDropdownOpen: false
        };
    }

    toggleDropdown = () => {
        this.setState((prevProps) => ({
            isDropdownOpen: !prevProps.isDropdownOpen
        }));
    }

    checkColumnsCountAndDisplayError = (): boolean => {
        const {columnsCount} = this.props;

        if(columnsCount > maxAllowedColumnsCount) {
            Toastr.error({message: "Pro export do PDF je možné mít zobrazeny maximálně tři sloupce."});
            return false;
        }
        return true;
    }

    handleColorOptionClick = () => {
        if(this.checkColumnsCountAndDisplayError()) {
            this.props.exportToPDF(PdfTemplate.Color);
        }
    }

    handleBlackAndWhiteOptionClick = () => {
        if(this.checkColumnsCountAndDisplayError()) {
            this.props.exportToPDF(PdfTemplate.BlackAndWhite);
        }
    }

    render() {
        return <>
            <ButtonDropdown isOpen={this.state.isDropdownOpen} toggle={this.toggleDropdown}>
                <DropdownToggle color="primary" caret>
                    Exportovat do PDF
                </DropdownToggle>
                <DropdownMenu>
                    <DropdownItem onClick={this.handleColorOptionClick}>Pro barevný tisk</DropdownItem>
                    <DropdownItem onClick={this.handleBlackAndWhiteOptionClick}>Pro černobílý tisk</DropdownItem>
                </DropdownMenu>
            </ButtonDropdown>
            {
                this.props.PDFUrl && <DownloadHiddenLink {...this.props} />
            }
        </>
    }
}

export default ExportToPDFButton;