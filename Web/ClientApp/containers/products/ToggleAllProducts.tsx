import * as React from 'react';
import { connect } from 'react-redux';
import * as ProductVariantsStore from '../../store/ProductVariants';
import { ApplicationState } from '../../store';
import ToggleAll from '../../components/common/ToggleAll';

type ToggleAllProductsProps =
    ProductVariantsStore.ProductVariantsState
    & typeof ProductVariantsStore.actionCreators

const ToggleAllProducts = (props: ToggleAllProductsProps) =>
    <ToggleAll isAnySelected={props.selectedProductVariants.length > 0} onSelectAll={props.selectAllProductVariants} onUnselectAll={props.unselectAllProductVariants} />

export default connect((state: ApplicationState) => state.productVariants, ProductVariantsStore.actionCreators)(ToggleAllProducts)