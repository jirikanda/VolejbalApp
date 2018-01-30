import * as React from "react";
import { ProductType } from "../../apimodels/SegmentsStore";
import { NavLink as RouterNavLink, Redirect } from "react-router-dom";
import { Nav, NavItem, NavLink } from "reactstrap";
import { LinkContainer } from "react-router-bootstrap";

interface ProductTypesProps {
  productTypes: ProductType[];
  segmentId: number;
  selectedProductTypeId: number;
  onSelectProductType: (productType: ProductType) => void;
}

export const ProductTypes = (props: ProductTypesProps) => {
    const {productTypes, segmentId, onSelectProductType, selectedProductTypeId} = props;
    const activeKey = selectedProductTypeId;

    if (activeKey) {
        return (
            <div className="container">
                <Nav tabs>
                    {props.productTypes.map(productType => (
                        <NavItem key={productType.id}>
                            <RouterNavLink onClick={() => onSelectProductType(productType)} className="nav-link" to={`/segments/${segmentId}/product-types/${productType.id}`}>
                                {productType.name}
                            </RouterNavLink>
                        </NavItem>
                    ))}
                </Nav>
            </div>
        );
    } else {
        const productTypeId = (productTypes[0] && productTypes[0].id) || 1;
        return (
            <Redirect
                exact
                to={`/segments/${segmentId}/product-types/${productTypeId}`}
            />
        );
    }
};