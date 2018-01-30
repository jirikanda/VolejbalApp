import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as ProductMatrixStore from '../../store/ProductMatrix';

type CancelSatisfactionRateLevelLinkProps =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators;

class CancelSatisfactionRateLevelLink extends React.Component<CancelSatisfactionRateLevelLinkProps> {
    render() {
        return <div className="mt-2">
            <a className="small" onClick={this.props.cancelSatisfactionRateLevel} style={{ cursor: "pointer" }}><u>Zrušit všechny barevné filtry</u></a>
        </div>;
    }
}

export default connect((state: ApplicationState) => state.productMatrix, ProductMatrixStore.actionCreators)(CancelSatisfactionRateLevelLink)