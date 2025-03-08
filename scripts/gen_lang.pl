#!/bin/perl

use strict;
use warnings;

my $content = qq(
	"game:placefailure-nopostpresent": "Cannot place this block here. Requires uninterrupted connection to post",
	"game:heldhelp-placesingle": "Place single",
	"game:heldhelp-placeauto": "Place multiple",

	"blockhelp-woodenpost-addtorchholder": "Add torch holder",
	
	"block-woodenpost-empty-*": "Wooden post",
);

my %woodtypes = (
	acacia => 'Acacia',
	aged => 'Aged',
	veryagedrotten => 'Rotten aged',
	veryaged => 'Very aged',
	agedebony => 'Aged ebony',
	alder => 'Alder',
	ash => 'Ash',
	azobe => 'Azobe',
	baldcypress => 'Bald cypress',
	banyan => 'Banyan', 
	bearnut => 'Bearnut',
	beech => 'Beech',
	birch => 'Birch',
	blackpoplar => 'Black poplar',
	bluemahoe => 'Blue mahoe', 
	brideinwhite => 'Bride',
	catalpa => 'Catalpa',
	cedar => 'Cedar',
	charred => 'Charred',
	chlorociboria => 'Chlorociboria', 
	dalbergia => 'Dalbergia', 
	douglasfir => 'Douglas fir',
	ebony => 'Ebony',
	elm => 'Elm',
	empresstree => 'Empress tree', 
	eucalyptus => 'Eucalyptus',
	fir => 'Fir',
	ginkgo => 'Ginko', 
	guajacum => 'Gaujacum', 
	ghostgum => 'Ghost gum', 
	honeylocust => 'Honey locust',
	horsechestnut => 'Chestnut',
	jacaranda => 'Jacaranda', 
	kauri => 'Kauri', 
	kapok => 'Kapok',
	larch => 'Larch',
	leadwood => 'Leadwood',
	linden => 'Linden',
	mahogany => 'Mahogany',
	mangrove => 'Mangrove',
	maple => 'Maple',
	oak => 'Oak',
	ohia => 'Ohia', 
	petrified => 'Petrified',
	pine => 'Pine',
	poplar => 'Poplar', 
	purpleheart => 'Purpleheart',
	pyramidalpoplar => 'Pyramidal poplar',
	redcedar => 'Red cedar', 
	redwood => 'Redwood',	
	rottenebony => 'Rotten ebony',
	sal => 'Sal',
	sapele => 'Sapele',
	satinash => 'Satinash', 
	saxaul => 'Saxaul',
	spruce => 'Spruce',
	spurgetree => 'Spurge',
	sycamore => 'Sycamore',
	walnut => 'Walnut',
	willow => 'Willow',
	tamanu => 'Tamanu',
	tigerwood => 'Tigerwood',
	tuja => 'Tuja',
	umnini => 'Umnini', 
	yew => 'Yew', 	
);

printf "{\n";

printf "$content";

printf "\n";

foreach my $woodtype (sort (keys %woodtypes)) {
	my $en = $woodtypes{$woodtype};
	printf "\t\"block-woodenpost-${woodtype}-barked-*\": \"Wooden post (${en})\",\n";
	printf "\t\"block-woodenposttorchholder-${woodtype}-barked-*\": \"Wooden post with torch holder (${en})\",\n";
	printf "\t\"block-woodenbeam-${woodtype}-barked-*\": \"Wooden beam (${en})\",\n";
	printf "\t\"block-decorbeam-${woodtype}-barked-*\": \"Decorative wooden beam (${en})\",\n";
	printf "\n";
	printf "\t\"block-woodenpost-${woodtype}-debarked-*\": \"Debarked wooden post (${en})\",\n";
	printf "\t\"block-woodenposttorchholder-${woodtype}-debarked-*\": \"Debarked wooden post with torch holder (${en})\",\n";
	printf "\t\"block-woodenbeam-${woodtype}-debarked-*\": \"Debarked wooden beam (${en})\",\n";
	printf "\t\"block-decorbeam-${woodtype}-debarked-*\": \"Debarked decorative wooden beam (${en})\",\n";
	printf "\n";
}

printf "\n}";
