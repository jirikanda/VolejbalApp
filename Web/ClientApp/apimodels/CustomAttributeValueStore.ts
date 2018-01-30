/* tslint:disable */

	export interface CustomAttributeValueCurrentIM {
		customAttributeId: number;
		feedback: string;
		productVariantId: number;
		satisfactionRatePercent: number;
		text: string;
		valueBoolean: boolean;
		valueBooleanExtended: BooleanExtended;
		valueDateTime: Date;
		valueDecimal: number;
		valueInt: number;

		/**
		 * x - irelevantní
		 */
		valueIsIrrelevant: boolean;

		/**
		 * NESTANOVENO
		 */
		valueIsNotSet: boolean;
		valueString: string;
	}

	/**
	 * Hodnoty pro aktualizaci draft-hodnoty.
	 */
	export interface CustomAttributeValueDraftIM extends CustomAttributeValueCurrentIM {
		approve: boolean;
		publishedFrom: Date;
		validFrom: Date;
	}
	export interface CustomAttributeValueVM {
		approved: Date;
		approvedBy: string;
		customAttributeValueId: number;
		feedback: string;
		publishedFrom: Date;
		satisfactionRatePercent: number;
		text: string;
		validFrom: Date;
		valueBoolean: boolean;
		valueBooleanExtended: BooleanExtended;
		valueDateTime: Date;
		valueDecimal: number;
		valueInt: number;
		valueIsIrrelevant: boolean;
		valueIsNotSet: boolean;
		valueString: string;
	}
	export interface CustomAttributeVM {
		measureUnit: string;
		name: string;
		symbol: CustomAttributeType;
		type: string;
		usesSatisfactionRate: boolean;
	}
	export interface GetCustomAttributeValueDetailIM {
		customAttributeId: number;
		productVariantId: number;
	}
	export interface GetCustomAttributeValueDetailVM {
		current: CustomAttributeValueVM;
		customAttribute: CustomAttributeVM;
		draft: CustomAttributeValueVM;
		hasPermissionToApprove: boolean;
		productVariantName: string;
		topicPath: string;
	}
	export enum BooleanExtended {
		True = 1,
		False = 2,
		Limited = 3
	}
	export enum CustomAttributeType {
		Text = 1,
		Integer = 2,
		Decimal = 3,
		Boolean = 4,
		BooleanExtended = 5,
		Date = 6
	}
