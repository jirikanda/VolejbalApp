/* tslint:disable */

	export interface ProfileVM {
		loginAccountName: string;
		partnerId: number;
		partners: Partner[];
		username: string;
	}
	export interface Branding {
		headerGradientBottomColor: string;
		headerGradientTopColor: string;
		logoUrl: string;
	}
	export interface Partner {
		branding: Branding;
		partnerCode: string;
		partnerId: number;
		partnerName: string;
	}
