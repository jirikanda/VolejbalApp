import * as React from 'react';
import { ProductVariant } from '../../apimodels/ProductMatrixStore';
import { connect } from 'react-redux';
import UnselectProductVariantComponent from '../../components/productMatrix/UnselectProductVariantComponent';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import { ApplicationState } from '../../store';

type UnselectProductVariantProps =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & { productVariant: ProductVariant }

const UnselectProductVariant = (props: UnselectProductVariantProps) => {
    const { productVariant, unselectProductVariant, requestCustomAttributes, requestCustomAttributesQuickSelection, selectedProductType, isQuickSelection } = props;

    return <UnselectProductVariantComponent
        unselectProductVariant={productVariantToUnselect => {
            unselectProductVariant(productVariantToUnselect);
            isQuickSelection ? requestCustomAttributesQuickSelection(selectedProductType.id) : requestCustomAttributes(selectedProductType.id)
        }}
        productVariant={productVariant} />
}

export default connect((state: ApplicationState, props: { productVariant: ProductVariant }) => state.productMatrix, ProductMatrixStore.actionCreators)(UnselectProductVariant)