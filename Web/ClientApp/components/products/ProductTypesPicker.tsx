import * as React from 'react';
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem } from 'reactstrap';
import { ProductTypeVM } from '../../apimodels/ProductTypesStore';

interface ProductTypesPickerProps {
    productTypes: ProductTypeVM[];
    productType: ProductTypeVM;
    onSelectedProductTypeChanged: (productType: ProductTypeVM) => void
}

interface ProductTypesPickerState {
    isOpen: boolean;
}

export class ProductTypesPicker extends React.Component<ProductTypesPickerProps, ProductTypesPickerState> {
    constructor(props: ProductTypesPickerProps) {
        super(props);

        this.state = {
            isOpen: false
        }
    }

    toggle = () => this.setState((prevState) => ({ isOpen: !prevState.isOpen }));

    render() {
        return <Dropdown isOpen={this.state.isOpen} toggle={this.toggle}>
            <DropdownToggle caret>
                {this.props.productType && this.props.productType.name}
            </DropdownToggle>
            <DropdownMenu>
                {this.props.productTypes && this.props.productTypes.map(productType =>
                    <DropdownItem key={productType.id} onClick={e => this.props.onSelectedProductTypeChanged(productType)}>{productType.name}</DropdownItem>)
                }
            </DropdownMenu>
        </Dropdown>
    }
}