import * as moment from 'moment';

/**
 * get date in format that is used in datetime picker
 */
export const getInputDateFormat = (date: Date): string => moment(date).format("YYYY-MM-DD");

/**
 * get date in format for UI representation
 */
export const getReadDateFormat = (date: Date): string => moment(date).format("l");

export const isDateInPast = (date: Date): boolean => moment(date).diff(new Date()) < 0;

/**
 * result is equal to date1 - date2
 */
export const getDateDiff = (date1: Date, date2: Date): number => moment(date1).diff(date2);