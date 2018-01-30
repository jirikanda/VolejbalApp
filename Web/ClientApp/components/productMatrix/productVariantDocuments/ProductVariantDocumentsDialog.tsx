import * as React from 'react';
import { Button } from 'reactstrap';
import Modal from '../../common/Modal';
import ProductVariantDocumentsTable from './ProductVariantDocumentsTable';
import { ProductVariantDocument, ProductVariantDocumentFile } from '../../../store/ProductVariantDocuments';

interface ProductVariantDocumentsDialogProps {
    productVariantDocuments: ProductVariantDocument[];
    historicalProductVariantDocuments: ProductVariantDocument[];
    isOpen: boolean;
    toggleModal: () => void;
    deleteProductVariantDocuments: (productVariantDocument: ProductVariantDocument) => void;
    saveProductVariantDocumentsMetadata: (productVariantDocument: ProductVariantDocument, newValidFrom: Date, newValidTo?: Date) => void;
    changeProductVariantDocumentsOrder: (hoverIndex: number, currentItemId: number) => void;
    uploadProductVariantDocuments: () => void;
    addProductVariantDocumentFile: (newDocument: ProductVariantDocumentFile) => void;
}

interface ProductVariantDocumentsDialogState {
    areHistorycalDocumentsDisplayed: boolean;
}

export default class ProductVariantDocumentsDialog extends React.Component<ProductVariantDocumentsDialogProps, ProductVariantDocumentsDialogState> {
    constructor(props: ProductVariantDocumentsDialogProps) {
        super(props)

        this.state = {
            areHistorycalDocumentsDisplayed: false
        }
    }

    handleToggleHistorycalDocumetsClick = () => {
        this.setState((state) => ({ areHistorycalDocumentsDisplayed: !state.areHistorycalDocumentsDisplayed }));
    }

    handleSaveChangesClick = () => {
        this.props.uploadProductVariantDocuments();
    }

    render() {
        const {isOpen, toggleModal, historicalProductVariantDocuments, productVariantDocuments} = this.props;
        const {areHistorycalDocumentsDisplayed} = this.state

        return <Modal dialogSize="lg"
            isOpen={isOpen}
            title={"Soubory metodik"}
            handleDialogClose={toggleModal}>
            <ProductVariantDocumentsTable {...this.props} editable />
            {
                historicalProductVariantDocuments.length > 0
                    && <Button color="primary"
                        outline
                        className="mb-3"
                        onClick={this.handleToggleHistorycalDocumetsClick}>
                        {
                            areHistorycalDocumentsDisplayed
                                ? "Skrýt historické soubory"
                                : "Zobrazit historické soubory"
                        }
                    </Button>
            }
            {
                areHistorycalDocumentsDisplayed
                    && <ProductVariantDocumentsTable {...this.props} deletable productVariantDocuments={historicalProductVariantDocuments} />
            }
            <Button onClick={this.handleSaveChangesClick} color="primary" outline className="pull-right">
                Uložit změny
            </Button>
        </Modal>
    }
}