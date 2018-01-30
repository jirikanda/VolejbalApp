import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as ProductVariants from '../../store/ProductVariants';
import SelectedProducersLoadButton from '../../components/products/SelectedProducersLoadButton';

type SelectedProducersLoadButtonContainerProps =
    ProductVariants.ProductVariantsState
    & typeof ProductVariants.actionCreators
    & { selectedProductTypeId: number }

class SelectedProducersLoadButtonContainer extends React.Component<SelectedProducersLoadButtonContainerProps> {
    render() {
        return <SelectedProducersLoadButton {...this.props} />
    }
}

export default connect((state: ApplicationState, props: { selectedProductTypeId: number }) => state.productVariants, ProductVariants.actionCreators)(SelectedProducersLoadButtonContainer)