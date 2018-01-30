/* tslint:disable */


	/**
	 * Parametry pro poskytnutí dat pro product matrix.
	 */
	export interface GetProductMatrixIM {

		/**
		 * Omezí navrácené custom attributes jen na ty, které jsou v Quick Selection.
		 */
		limitToQuickSelection: boolean;

		/**
		 * Varianty produktů (tarify), které mají být v product matrix.
		 */
		productVariantIds: number[];

		/**
		 * Topics, jejichž rozšiřující podmínky mají být v product matrix.
		 */
		topicIds: number[];
	}
	export interface PdfExportIM {
		customAttributeIds: number[];
		pdfTemplate: PdfTemplate;
		productVariantIds: number[];
	}
	export interface PdfExportVM {
		data: string;
	}
	export interface PostFeedbackIM {
		customAttributeId: number;
		feedback: string;
		productVariantId: number;
	}

	/**
	 * Product Matrix. Data produktové matice.
	 */
	export interface ProductMatrixVM {

		/**
		 * Custom attributes témat. Obsahuje též hodnoty.
		 */
		customAttributes: CustomAttribute[];

		/**
		 * Rozlišení, zda daný uživatel má právo editovat odpovědi.
		 */
		hasPermissionsToEdit: boolean;

		/**
		 * Product variants.
		 */
		productVariants: ProductVariant[];

		/**
		 * Seznam úrovní satisfaction rate. Poskytuje seznam css tříd pro poskytnutí filtru dle míry uspokojení.
		 */
		satisfactionRateLevels: SatisfactionRateLevel[];

		/**
		 * Seznam témat v hierarchii. Obsahuje všechna témata od kořenových po ta témata, která byla v dotazu.
            Pokud se ptáme na topic 3, ten má parenta 2, ten parenta 1, pak jsou v kolekci topic 1, 2 i 3.
		 */
		topics: Topic[];
	}

	/**
	 * Rozšiřující podmínka.
	 */
	export interface CustomAttribute {

		/**
		 * US-108 - v matici odpovědí zobrazit správci dat indikátor zdali je upřesňující podmínka součástí Rychlého výběru
		 */
		displayIncludedInQuickSelection: boolean;
		id: number;
		name: string;

		/**
		 * Téma, do kterého se rozšiřující podmínka váže.
		 */
		topicId: number;

		/**
		 * Hodnoty jednotlivých produktových variant.
		 */
		values: CustomAttributeValue[];
	}

	/**
	 * Hodnota rozšiřující podmínky pro konkrétní produktovou variantu.
	 */
	export interface CustomAttributeValue {

		/**
		 * Feedback (sdílená zkušenost, zpětná vazba k hodnotě atributu.
		 */
		feedback: string;

		/**
		 * ProductVarianta, ke které se odpověď váže.
		 */
		productVariantId: number;

		/**
		 * CssClass určená dle SafisfactionRate (z SafisfactionRateLevel).
		 */
		satisfactionRateCssClass: string;

		/**
		 * US-107 - v matici odpovědí zobrazit míru uspokojení u každé z odpovědí
            Míra uspokojení (jako text).
            Pokud nemá být zobrazena, je null.
		 */
		satisfactionRateText: string;

		/**
		 * Textová reprezentace odpovědi.
		 */
		text: string;

		/**
		 * Od kdy je hodnota platná.
		 */
		validFrom: Date;

		/**
		 * Indikuje, že otázka je irrelevantní pro tuto variantu produktu.
		 */
		valueIsIrrelevant: boolean;

		/**
		 * Indikuje, že otázka nemá hodnotu pro tuto variantu produktu (neexistuje CustomAttributeValue).
		 */
		valueIsMissing: boolean;

		/**
		 * Indikuje, že otázka nemá uvedenou hodnotu pro tuto variantu produktu (existuje CustomAttributeValue, avšak její ValueXxx má hodnotu null).
		 */
		valueIsNotSet: boolean;

		/**
		 * Indikuje, zda došlo v posledních X měsících k aktualizaci hodnoty.
		 */
		wasRecentlyUpdated: boolean;
	}

	/**
	 * Produktová varianta.
	 */
	export interface ProductVariant {
		id: number;

		/**
		 * Podlední datum ValidFrom z hodnot produkt varianty. Globálně přes všechny odpovědi dané varianty produktu (tj. úmyslně nebere v úvahu jen zobrazené rozšiřující podmínky dle topics, ale bere v úvahu hodnoty všechn podmínek).
		 */
		lastValidFrom: Date;
		name: string;

		/**
		 * Indikuje, zda došlo v poslední době ke změně nějaké hodnoty u varianty produktu.
		 */
		wasRecentlyUpdated: boolean;
	}

	/**
	 * Úroveň míry uspokojení.
	 */
	export interface SatisfactionRateLevel {

		/**
		 * CssClass pro zobrazení dané úrovně míry uspokojení.
		 */
		cssClass: string;
	}

	/**
	 * Téma.
	 */
	export interface Topic {

		/**
		 * Podtémata.
		 */
		children: Topic[];
		id: number;
		name: string;
	}
	export enum PdfTemplate {
		BlackAndWhite = 2,
		Color = 1
	}
