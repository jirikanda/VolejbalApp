import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as ProductVariants from '../../store/ProductVariants';
import SelectedProducersSaveButton from '../../components/products/SelectedProducersSaveButton';

type SelectedProducersSaveButtonContainerProps =
    ProductVariants.ProductVariantsState
    & typeof ProductVariants.actionCreators
    & { selectedProductTypeId: number }

class SelectedProducersSaveButtonContainer extends React.Component<SelectedProducersSaveButtonContainerProps> {
    render() {
        return <SelectedProducersSaveButton {...this.props} />
    }
}

export default connect((state: ApplicationState, props: { selectedProductTypeId: number }) => state.productVariants, ProductVariants.actionCreators)(SelectedProducersSaveButtonContainer)