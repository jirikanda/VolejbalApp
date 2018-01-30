/* tslint:disable */


	/**
	 * Segment.
	 */
	export interface SegmentDetailVM {
		id: number;
		name: string;
	}

	/**
	 * Segment.
	 */
	export interface SegmentVM {

		/**
		 * Indikuje, zda má uživatel oprávnění k Editace hierarchie Témat, zakládání/editace/mazání Témat/Upřesňujících podmínek v daném segmentu.
		 */
		hasPermissionToEdit: boolean;

		/**
		 * Indikuje, zda má uživatel oprávnění ke čtení daného segmentu.
		 */
		hasPermissionToRead: boolean;
		id: number;

		/**
		 * Název obrázku pro zobrazení segmentu.
		 */
		image: string;

		/**
		 * Indikuje, zda uživatel uvidí segment.
		 */
		isVisible: boolean;
		name: string;

		/**
		 * Seznam productových typů segmentu.
		 */
		productTypes: ProductType[];
	}
	export interface ProductType {
		id: number;
		name: string;
	}
