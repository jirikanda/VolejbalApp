/* tslint:disable */


	/**
	 * Dokumenty k produktovým variantám.
	 */
	export interface DocumentsVM {

		/**
		 * Platné dokumenty.
		 */
		documents: Document[];

		/**
		 * Indikuje, zda může uživatel soubory editovat.
		 */
		hasPermissionToEdit: boolean;

		/**
		 * Historické dokumenty.
		 */
		historicalDocuments: Document[];
	}
	export interface PostDocumentIM {

		/**
		 * Base64
		 */
		content: string;
		id: number;
		name: string;
		validFrom: Date;
		validTo: Date;
	}
	export interface PostDocumentsIM {
		documents: PostDocumentIM[];
		historicalDocuments: PostHistoricalDocumentIM[];
	}
	export interface PostHistoricalDocumentIM {
		id: number;
	}
	export interface Document {
		id: number;
		name: string;
		url: string;
		validFrom: Date;
		validTo: Date;
	}
