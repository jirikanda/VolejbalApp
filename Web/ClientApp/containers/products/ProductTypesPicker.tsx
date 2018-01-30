import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store/index';
import * as ProductTypesStore from '../../store/ProductTypes';
import * as TopicsStore from '../../store/Topics';
import { ProductTypesPicker as ProductTypesPickerComponent } from '../../components/products/ProductTypesPicker';
import { SegmentVM } from '../../apimodels/SegmentsStore';
import { ProductTypeVM } from '../../apimodels/ProductTypesStore';

type ProductTypesPickerProps =
    ProductTypesStore.ProductTypesState
    & typeof ProductTypesStore.actionCreators
    & typeof TopicsStore.actionCreators
    & { segment: SegmentVM; productTypes: ProductTypesStore.ProductTypesState }

class ProductTypesPicker extends React.Component<ProductTypesPickerProps> {
    componentWillMount() {
        if (this.props.segment) {
            this.props.requestProductTypes(this.props.segment.id);
        }
    }
    componentWillReceiveProps(nextProps: ProductTypesPickerProps) {
        if (nextProps.segment && (this.props.segment == undefined || nextProps.segment.id !== this.props.segment.id)) {
            this.props.requestProductTypes(nextProps.segment.id);
        }
    }

    onSelectedProductTypeChanged = (productType: ProductTypeVM) => {
        this.props.selectProductType(productType);
        this.props.requestTopics(productType.id);
    }

    render() {
        return <ProductTypesPickerComponent {...this.props} onSelectedProductTypeChanged={this.onSelectedProductTypeChanged} />
    }
}

export default connect((state: ApplicationState) => ({
    segment: state.segments.segment,
    productTypes: state.productTypes.productTypes,
    productType: state.productTypes.productType
}), {...ProductTypesStore.actionCreators, ...TopicsStore.actionCreators})(ProductTypesPicker)