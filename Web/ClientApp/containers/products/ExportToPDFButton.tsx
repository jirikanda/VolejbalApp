import * as React from 'react';
import { connect } from 'react-redux';

import { ApplicationState } from '../../store';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import ExportToPDFButtonComponent from '../../components/productMatrix/ExportToPDFButton';

type ExportToPDFButtonProps = ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators

class ExportToPDFButton extends React.Component<ExportToPDFButtonProps> {
    render() {
        const { productVariants } = this.props;

        return <ExportToPDFButtonComponent {...this.props} columnsCount={productVariants.length} />
    }
}

export default connect(
    (state: ApplicationState) => state.productMatrix,
    ProductMatrixStore.actionCreators
)(ExportToPDFButton);
