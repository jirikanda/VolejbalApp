import * as React from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router';
import { RouteComponentProps } from 'react-router-dom';
import { bindActionCreators } from 'redux';

import { ProductTypes as ProductTypesComponent } from '../../components/products/ProductTypes';
import { ApplicationState } from '../../store';
import * as ProductTypeStore from '../../store/ProductTypes';
import * as TopicsStore from '../../store/Topics';

type ProductTypesProps =
    ProductTypeStore.ProductTypesState
    & typeof ProductTypeStore.actionCreators
    & typeof TopicsStore.actionCreators
    & RouteComponentProps<{ segmentId: number; productTypeId: number }>;

class ProductTypes extends React.Component<ProductTypesProps> {
    componentWillMount() {
        this.props.requestProductTypes(this.props.match.params.segmentId)
    }

    componentDidUpdate(prevProps: ProductTypesProps) {
        if (this.props.match.params.productTypeId !== prevProps.match.params.productTypeId) {
            this.props.unselectAllTopics();
        }
    }

    render() {
        return <ProductTypesComponent
            productTypes={this.props.productTypes}
            segmentId={this.props.match.params.segmentId}
            selectedProductTypeId={this.props.match.params.productTypeId}
            onSelectProductType={this.props.selectProductType} />
    }
}

const mapStateToProps = (state: ApplicationState, props: RouteComponentProps<{ segmentId: number; productTypeId: number }>) => state.productTypes;

const mapActionCreatorsToProps = (dispatch, ownProps) => {
    return {
        ...bindActionCreators(ProductTypeStore.actionCreators, dispatch),
        ...bindActionCreators(TopicsStore.actionCreators, dispatch)
    }
};

export default withRouter(connect(mapStateToProps, mapActionCreatorsToProps)(ProductTypes));