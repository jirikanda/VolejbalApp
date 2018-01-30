import * as React from "react";
import { ProductVariant } from "../../apimodels/ProductMatrixStore";

interface UnselectProductVariantComponentProps {
  productVariant: ProductVariant;
  unselectProductVariant: (productVariant: ProductVariant) => void;
}

const UnselectProductVariantComponent = (
  props: UnselectProductVariantComponentProps
) => {
  const { productVariant, unselectProductVariant } = props;

  return (
    <span onClick={e => unselectProductVariant(productVariant)}>
      <i
        className="fa fa-trash-o float-right mt-1 pointer"
        aria-hidden="true"
      />
    </span>
  );
};

export default UnselectProductVariantComponent;
