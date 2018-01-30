import * as React from 'react';
import { connect } from 'react-redux';
import * as ProductTypeStore from '../../store/ProductTypes';
import { RouteComponentProps, withRouter } from 'react-router-dom';
import { ApplicationState } from '../../store';

type ProductTypeNameProps =
    ProductTypeStore.ProductTypesState
    & typeof ProductTypeStore.actionCreators
    & { productTypeId: string };

class ProductTypeName extends React.Component<ProductTypeNameProps> {
    render() {
        const { productTypes } = this.props;
        const productTypeId = parseInt(this.props.productTypeId);
        const currentProdType = productTypes.find(prodType => prodType.id === productTypeId);

        return currentProdType && currentProdType.name
    }
}

export default connect((state: ApplicationState) => state.productTypes, ProductTypeStore.actionCreators)(ProductTypeName);