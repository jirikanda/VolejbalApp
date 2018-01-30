import * as React from 'react';
import { Button } from 'reactstrap';
import Modal from '../common/Modal';
import ProductVariants from '../../containers/products/ProductVariants';

interface ProductVariantsModalDialogProps {
    selectedProductTypeId: number;
    isProductVariantsModalOpen: boolean;
    toggleProductVariantsModal: () => void;
    applyAndClose: () => void;
}

const ProductVariantsModalDialog = (props: ProductVariantsModalDialogProps) => {
    const {isProductVariantsModalOpen, toggleProductVariantsModal, selectedProductTypeId, applyAndClose} = props;

    return <Modal dialogSize="lg"
        footer={
            <Button color="primary" onClick={applyAndClose}>Upravit</Button>
        }
        isOpen={props.isProductVariantsModalOpen}
        title={"Produkt"}
        handleDialogClose={toggleProductVariantsModal}>
        <ProductVariants selectedProductTypeId={selectedProductTypeId} />
    </Modal>
}

export default ProductVariantsModalDialog;