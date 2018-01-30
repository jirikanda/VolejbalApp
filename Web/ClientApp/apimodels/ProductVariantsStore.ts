/* tslint:disable */


	/**
	 * Produktové varianty.
	 */
	export interface ProductVariantsVM {

		/**
		 * Historičtí producenti.
		 */
		historicalProducers: Producer[];

		/**
		 * Producenti.
		 */
		producers: Producer[];
	}

	/**
	 * Producent.
	 */
	export interface Producer {
		id: number;
		name: string;

		/**
		 * Produktové varianty.
		 */
		productVariants: ProductVariant[];
	}

	/**
	 * Produktové varianty.
	 */
	export interface ProductVariant {
		id: number;
		name: string;
	}
