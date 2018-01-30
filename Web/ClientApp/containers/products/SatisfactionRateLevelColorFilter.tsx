import * as React from 'react';
import * as ProductMatrixStore from '../../store/ProductMatrix';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import SatisfactionRateLevelColorFilterComponent from '../../components/productMatrix/SatisfactionRateLevelColorFilterComponent'
import { CustomAttribute, ProductVariant } from '../../apimodels/ProductMatrixStore'

interface SatisfactionRateLevelColorFilterProps {
    isSatisfactionRateLevelFilterVisible: boolean;
    customAttribute: CustomAttribute;
}

type SatisfactionRateLevelColorFilterPropsPublic =
    ProductMatrixStore.ProductMatrixState
    & typeof ProductMatrixStore.actionCreators
    & SatisfactionRateLevelColorFilterProps;

class SatisfactionRateLevelColorFilter extends React.Component<SatisfactionRateLevelColorFilterPropsPublic> {
    constructor(props) {
        super(props);
    }

    render() {
        const { isSatisfactionRateLevelFilterVisible } = this.props;

        return <div>{ isSatisfactionRateLevelFilterVisible && <SatisfactionRateLevelColorFilterComponent {...this.props} /> }</div>;
    }
}

export default connect((state: ApplicationState, props: SatisfactionRateLevelColorFilterProps) => state.productMatrix, ProductMatrixStore.actionCreators)(SatisfactionRateLevelColorFilter)