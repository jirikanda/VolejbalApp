import * as React from 'react';
import { connect } from 'react-redux';
import { ProductVariants as ProductVariantsComponent } from '../../components/products/ProductVariants';
import * as ProductVariantsStore from '../../store/ProductVariants';
import { ApplicationState } from '../../store';

type PublicProductVariantsProps = { selectedProductTypeId: number };

type ProductVariantsProps =
    ProductVariantsStore.ProductVariantsState
    & typeof ProductVariantsStore.actionCreators
    & PublicProductVariantsProps;

class ProductVariants extends React.Component<ProductVariantsProps> {
    componentWillMount() {
        this.props.requestProductVariants(this.props.selectedProductTypeId, !location.pathname.includes("product-matrix"));
    }

    componentWillReceiveProps(props: ProductVariantsProps) {
        if (this.props.selectedProductTypeId !== props.selectedProductTypeId) {
            this.props.requestProductVariants(props.selectedProductTypeId, !location.pathname.includes("product-matrix"));
        }
    }

    render() {

        const { selectedProductTypeId } = this.props;

        return <div>{
            selectedProductTypeId && <ProductVariantsComponent
                producers={this.props.producers}
                selectedProductVariants={this.props.selectedProductVariants}
                onProductVariantSelect={this.props.selectProductVariant}
                onProductVariantUnselect={this.props.unselectProductVariant}
                onProductVariantsSelect={this.props.selectProductVariants}
                onProductVariantsUnselect={this.props.unselectProductVariants}
            />
        }</div>
    }
}

export default connect((state: ApplicationState, props: PublicProductVariantsProps) => state.productVariants, ProductVariantsStore.actionCreators)(ProductVariants);