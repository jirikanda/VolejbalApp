import * as React from 'react';
import { connect } from 'react-redux';

import ProductVariantDocumentsDialog from '../../components/productMatrix/productVariantDocuments/ProductVariantDocumentsDialog';
import { ApplicationState } from '../../store';
import * as ProductVariantDocumentsStore from '../../store/ProductVariantDocuments';
import { ProductVariant } from './../../apimodels/ProductMatrixStore';
import { Topic } from './../../apimodels/TopicsStore';

interface PublicProductVariantDocumentsProps {
    productVariants: ProductVariant[];
    isProductVariantDocumentsModalOpen: boolean;
    toggleProductVariantDocumentsModal: () => void;
};

type ProductVariantDocumentsProps =
    ProductVariantDocumentsStore.ProductVariantDocumentsState
    & { selectedTopics: Topic[] }
    & typeof ProductVariantDocumentsStore.actionCreators
    & PublicProductVariantDocumentsProps;

class ProductVariantDocuments extends React.Component<ProductVariantDocumentsProps> {

    render() {
        const { isProductVariantDocumentsModalOpen, toggleProductVariantDocumentsModal } = this.props;

        return <ProductVariantDocumentsDialog
            {...this.props}
            isOpen={isProductVariantDocumentsModalOpen}
            toggleModal={toggleProductVariantDocumentsModal} />;
    }
}

const mapStateToProps = (state: ApplicationState, props: PublicProductVariantDocumentsProps) => {
    return {
        ...state.productVariantDocuments,
        selectedTopics: state.productMatrix.selectedTopics
    }
}

export default connect(
    mapStateToProps,
    ProductVariantDocumentsStore.actionCreators
)(ProductVariantDocuments);
