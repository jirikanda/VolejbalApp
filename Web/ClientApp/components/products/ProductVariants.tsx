import * as React from 'react';
import { ProductVariant, Producer } from '../../apimodels/ProductVariantsStore';
import CheckboxDropDownList from '../common/CheckboxDropDownList';

type ProductVariantsProps = {
    producers: Producer[];
    selectedProductVariants: ProductVariant[];
    onProductVariantSelect: (productVariant: ProductVariant) => void;
    onProductVariantUnselect: (productVariant: ProductVariant) => void;
    onProductVariantsSelect: (productVariants: ProductVariant[]) => void;
    onProductVariantsUnselect: (productVariants: ProductVariant[]) => void;
}

export const ProductVariants = (props: ProductVariantsProps) => {
    const onChange = (checked: boolean, productVariant: ProductVariant) => checked ? props.onProductVariantSelect(productVariant) : props.onProductVariantUnselect(productVariant)

    return <ul className="product-filter">{props.producers && props.producers.length > 0 && props.producers.map((producer, index) => {
        if (producer.productVariants.length === 1) {
            const productVariant = producer.productVariants[0];
            const productVariantCheckBoxId = `productVariantCheckbox-${productVariant.id}`;
            return <li key={index} className="product-filter-item">
                <input
                    id={productVariantCheckBoxId}
                    type="checkbox"
                    checked={props.selectedProductVariants.find(selectedProductVariant => selectedProductVariant.id === productVariant.id) !== undefined}
                    onChange={event => onChange(event.target.checked, productVariant)}/>
                <ProductVariantName name={producer.name} htmlFor={productVariantCheckBoxId} />
            </li>
        } else {
            return <li key={index} className="product-filter-item">
                <CheckboxDropDownList
                    title={producer.name}
                    items={producer.productVariants}
                    selectedItems={props.selectedProductVariants}
                    onItemSelect={props.onProductVariantSelect}
                    onItemUnselect={props.onProductVariantUnselect}
                    onItemsSelect={props.onProductVariantsSelect}
                    onItemsUnselect={props.onProductVariantsUnselect} />
            </li>
        }
    })}</ul>
}

type ProductVariantNameProps = {
    name: string;
    htmlFor: string;
}

const ProductVariantName = (props: ProductVariantNameProps) => {
    return <label htmlFor={props.htmlFor}>{props.name}</label>
}